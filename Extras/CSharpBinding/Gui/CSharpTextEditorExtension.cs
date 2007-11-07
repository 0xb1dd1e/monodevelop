using System;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;

using MonoDevelop.Projects.Parser;
using MonoDevelop.Projects.Gui.Completion;

using MonoDevelop.SourceEditor;
using MonoDevelop.SourceEditor.FormattingStrategy;

using CSharpBinding.Parser;
using CSharpBinding.FormattingStrategy;

using MonoDevelop.Projects.Ambience;
using Ambience_ = MonoDevelop.Projects.Ambience.Ambience;

namespace CSharpBinding
{
	public class CSharpTextEditorExtension: CompletionTextEditorExtension
	{
		CSharpIndentEngine indentEngine = new CSharpIndentEngine ();
		Stack< CSharpIndentEngine> oldEngines = new Stack<CSharpIndentEngine> ();
		
		public CSharpTextEditorExtension () : base ()
		{
		}
		
		public override bool ExtendsEditor (Document doc, IEditableTextBuffer editor)
		{
			return System.IO.Path.GetExtension (doc.Title) == ".cs";
		}
		
		IClass LookupClass (ICompilationUnit unit, int line, int column)
		{
			if (unit.Classes != null) {
				int startLine = int.MaxValue;
				IClass result = null;
				foreach (IClass c in unit.Classes) {
					if (c.Region != null && c.Region.BeginLine < startLine && c.Region.BeginLine > line) { 
						startLine = c.Region.BeginLine;						
						result = c.ClassType == ClassType.Delegate ? c : null;
					}
					if (c.BodyRegion != null && c.BodyRegion.IsInside (line, column))
						return c;
				}
				return result;
			}
			return null;
		}
		
		void AppendSummary (StringBuilder sb, string indent, out int newCursorOffset)
		{
			Debug.Assert (sb != null);
			sb.Append ("/ <summary>\n");
			sb.Append (indent);
			sb.Append ("/// \n");
			sb.Append (indent);
			sb.Append ("/// </summary>");
			newCursorOffset = ("/ <summary>\n/// " + indent).Length;
		}
		
		void AppendMethodComment (StringBuilder builder, string indent, IMethod method)
		{
			if (method.Parameters != null) {
				foreach (IParameter para in method.Parameters) {
					builder.Append (Environment.NewLine);
					builder.Append (indent);
					builder.Append ("/// <param name=\"");
					builder.Append (para.Name);
					builder.Append ("\">\n");
					builder.Append (indent);
					builder.Append ("/// A <see cref=\"");
					builder.Append (para.ReturnType.FullyQualifiedName);
					builder.Append ("\"/>\n");
					builder.Append (indent);
					builder.Append ("/// </param>");
				}
			}
			if (method.ReturnType != null && method.ReturnType.FullyQualifiedName != "System.Void") {
				builder.Append (Environment.NewLine);
				builder.Append (indent);
				builder.Append("/// <returns>\n");
				builder.Append (indent);
				builder.Append ("/// A <see cref=\"");
				builder.Append (method.ReturnType.FullyQualifiedName);
				builder.Append ("\"/>\n");
				builder.Append (indent);
				builder.Append ("/// </returns>");
			}
		}
		
		string GenerateBody (IClass c, int line, string indent, out int newCursorOffset)
		{
			int startLine = int.MaxValue;
			newCursorOffset = 0;
			StringBuilder builder = new StringBuilder ();
			
			IMethod method = null;
			IProperty property = null;
			foreach (IMethod m in c.Methods) {
				if (m.Region.BeginLine < startLine && m.Region.BeginLine > line) {
					startLine = m.Region.BeginLine;
					method = m;
				}
			}
			foreach (IProperty p in c.Properties) {
				if (p.Region.BeginLine < startLine && p.Region.BeginLine > line) {
					startLine = p.Region.BeginLine;
					property = p;
				}
			}
			
			if (method != null) {
				AppendSummary (builder, indent, out newCursorOffset);
				AppendMethodComment (builder, indent, method);
			} else if (property != null) {
				builder.Append ("/ <value>\n");
				builder.Append (indent);
				builder.Append ("/// \n");
				builder.Append (indent);
				builder.Append ("/// </value>");
				newCursorOffset = ("/ <value>\n/// " + indent).Length;
			}
			
			return builder.ToString ();
		}
		
		bool IsInsideClassBody (IClass insideClass, int line, int column)
		{
			if (insideClass.Methods != null) {
				foreach (IMethod m in insideClass.Methods) {
					if (m.BodyRegion.IsInside (line, column)) {
						return false;
					}
				}
			}
			
			if (insideClass.Properties != null) {
				foreach (IProperty p in insideClass.Properties) {
					if (p.BodyRegion.IsInside (line, column)) {
						return false;
					}
				}
			}
			
			if (insideClass.Indexer != null) {
				foreach (IIndexer p in insideClass.Indexer) {
					if (p.BodyRegion.IsInside (line, column)) {
						return false;
					}
				}
			}
			return true;
		}
		
		bool MayNeedComment (int line, int cursor)
		{
			bool inComment = Editor.GetCharAt (cursor - 1) == '/' && Editor.GetCharAt (cursor - 2) == '/';
			
			if (inComment) {
				for (int l = line - 1; l >= 0; l--) {
					string text = Editor.GetLineText (l).Trim (); 
					if (text.StartsWith ("///"))
						return false;
					if (!String.IsNullOrEmpty (text))
						break;
				}
				for (int l = line + 1; l < line + 100; l++) {
					string text = Editor.GetLineText (l).Trim (); 
					if (text.StartsWith ("///"))
						return false;
					if (!String.IsNullOrEmpty (text))
						break;
				}
				return true;
			}
			return false;
		}
		
		public override void TextChanged (int startIndex, int endIndex)
		{
			if (startIndex < indentEngine.Cursor)
				ResetSmartIndentEngineToCursor (startIndex);
			base.TextChanged (startIndex, endIndex);
		}
		
		public override bool KeyPress (Gdk.Key key, Gdk.ModifierType modifier)
		{
			if ((char)(uint)key == ',') {
				// Parameter completion
				RunParameterCompletionCommand ();
			}
			
			if (GenerateDocComments (key))
				//if doc comments were inserted, further handling not necessary
				return false;
			
			//do the smart indent
			if (TextEditorProperties.IndentStyle == IndentStyle.Smart) {
				//pre text-insertion indent handling
				if (!DoPreKeySmartIndent (key))
					return false;
				
				//pass through to the base class, which actually inserts the character
				int oldBufLen = Editor.TextLength;
				bool retval = base.KeyPress (key, modifier);
				UpdateSmartIndentEngine ();
				
				//handle inserted characters
				bool reIndent = false;
				char lastCharInserted = KeyToChar (key);
				//System.Console.WriteLine (lastCharInserted);
				if (oldBufLen != Editor.TextLength || lastCharInserted != '\0')
					DoPostInsertionSmartIndent (lastCharInserted, out reIndent);
				
				//reindent the line after the insertion, if needed
				//N.B. if the engine says we need to reindent, make sure that it's because a char was 
				//inserted rather than just updating the stack due to moving around
				UpdateSmartIndentEngine ();
				if (reIndent || (indentEngine.NeedsReindent && lastCharInserted != '\0'))
					DoReSmartIndent ();
				return retval;
			}
			return base.KeyPress (key, modifier);
		}
		
		char KeyToChar (Gdk.Key key)
		{
			switch (key) {
			case Gdk.Key.Tab:
				return '\t';
			case Gdk.Key.KP_Enter:
			case Gdk.Key.Return:
				return '\n';
			default:
				return (char) Gdk.Keyval.ToUnicode ((uint)key);
			}
		}
		
		void ResetSmartIndentEngine ()
		{
			oldEngines.Clear ();
			indentEngine.Reset ();
		}
		
		void ResetSmartIndentEngineToCursor (int cursor)
		{
			bool gotOldEngine = false;
			while (oldEngines.Count > 0) {
				CSharpIndentEngine csie = oldEngines.Peek ();
				if (csie.Cursor <= cursor) {
					indentEngine = (CSharpIndentEngine) csie.Clone ();
					gotOldEngine = true;
					//System.Console.WriteLine("Recovered stack engine #{0}", oldEngines.Count);
					break;
				} else {
					oldEngines.Pop ();
				}
			}
			if (!gotOldEngine) {
				//System.Console.WriteLine("Did not recover a stack engine", oldEngines.Count);
				indentEngine.Reset ();
			}
		}
		
		//Makes sure that the smart indent engine's cursor has caught up with the 
		//text editor's cursor.
		//The engine can take some time to parse the file, and we need it to be snappy
		//so we keep a stack of old engines (they're fairly lightweight) that we can clone
		//in order to quickly catch up.
		void UpdateSmartIndentEngine ()
		{
			//bigger buffer means fewer saved stacks needed
			const int BUFFER_SIZE = 2000;
			
			int cursor = Editor.CursorPosition;
			//System.Console.WriteLine("moving backwards if indentEngine.Cursor {0} > cursor {1}", indentEngine.Cursor, cursor);
			
			if (indentEngine.Cursor == cursor) {
				//cursors match, nothing to be done
				return;
			} else if (indentEngine.Cursor > cursor) {
				//moving backwards, so reset from previous saved location
				ResetSmartIndentEngineToCursor (cursor);
			}
			
			// get the engine caught up
			int nextSave = (oldEngines.Count == 0)? BUFFER_SIZE : oldEngines.Peek ().Cursor + BUFFER_SIZE;
			if (indentEngine.Cursor + 1 == cursor) {
				char ch = Editor.GetCharAt (indentEngine.Cursor);
				indentEngine.Push (ch);
				//System.Console.WriteLine("pushing character '{0}'", ch);
				if (indentEngine.Cursor == nextSave)
					oldEngines.Push ((CSharpIndentEngine) indentEngine.Clone ());
			} else {
				//bulk copy characters in case buffer is unmanaged 
				//(faster if we reduce managed/unmanaged transitions)
				while (indentEngine.Cursor < cursor) {
					int endCut = indentEngine.Cursor + BUFFER_SIZE;
					if (endCut > cursor)
						endCut = cursor;
					string buffer = Editor.GetText (indentEngine.Cursor, endCut);
					//System.Console.WriteLine("getting buffer between {0} and {1} : '{2}'", indentEngine.Cursor, endCut - 1, buffer);
					foreach (char ch in buffer) {
						indentEngine.Push (ch);
						//System.Console.WriteLine("pushing character '{0}'", ch);
						if (indentEngine.Cursor == nextSave) {
							oldEngines.Push ((CSharpIndentEngine) indentEngine.Clone ());
							nextSave += BUFFER_SIZE;
						}
					}
				}
			}
			//System.Console.WriteLine("***now indentengine is at {0}, doc is at {1}", indentEngine.Cursor, cursor);
		}
		
		bool GenerateDocComments (Gdk.Key key)
		{
			int cursor;
			int newCursorOffset = 0;
			
			switch (key) {
			case Gdk.Key.greater:	
				cursor = Editor.SelectionStartPosition;
				if (IsInsideDocumentationComment (Editor.SelectionStartPosition)) {
					int lin, col;
					Editor.GetLineColumnFromPosition (Editor.CursorPosition, out lin, out col);
					string lineText = Editor.GetLineText (lin);
					int startIndex = Math.Min (col - 1, lineText.Length - 1);
					
					while (startIndex >= 0 && lineText[startIndex] != '<') {
						--startIndex;
						if (lineText[startIndex] == '/') { // already closed.
							startIndex = -1;
							break;
						}
					}
					if (startIndex >= 0) {
						int endIndex = startIndex;
						while (endIndex <= col && endIndex < lineText.Length && !Char.IsWhiteSpace (lineText[endIndex])) {
							endIndex++;
						}
						string tag = endIndex - startIndex - 1 > 0 ? lineText.Substring (startIndex + 1, endIndex - startIndex - 1) : null;
						if (!String.IsNullOrEmpty (tag) && commentTags.IndexOf (tag) >= 0) {
							Editor.InsertText (cursor, "></" + tag + ">");
							Editor.CursorPosition = cursor + 1; 
							return true;
						}
					}
				}
				break;
				
			case Gdk.Key.KP_Divide:
			case Gdk.Key.slash:
				cursor = Editor.SelectionStartPosition;
				if (cursor < 2)
					break;
				int lin, col;
				Editor.GetLineColumnFromPosition (Editor.CursorPosition, out lin, out col);
				
				if (MayNeedComment (lin, cursor)) {
					StringBuilder generatedComment = new StringBuilder ();
					bool generateStandardComment = true;
					IParserContext pctx = GetParserContext ();
					ICompilationUnit unit = pctx.ParseFile (this.FileName, this.Editor.Text).MostRecentCompilationUnit as ICompilationUnit;
					if (unit != null) {
						
						IClass insideClass = LookupClass (unit, lin, col);
						if (insideClass != null) {
							string indent = GetLineWhiteSpace (Editor.GetLineText (lin));
							if (insideClass.ClassType == ClassType.Delegate) {
								AppendSummary (generatedComment, indent, out newCursorOffset);
								AppendMethodComment (generatedComment, indent, insideClass.Methods[0]);
								generateStandardComment = false;
							} else {
								if (!IsInsideClassBody (insideClass, lin, col))
									break;
								string body = GenerateBody (insideClass, lin, indent, out newCursorOffset);
								if (!String.IsNullOrEmpty (body)) {
									generatedComment.Append (body);
									generateStandardComment = false;
								}
							}
						}
					}
					if (generateStandardComment) {
						string indent = GetLineWhiteSpace (Editor.GetLineText (lin));;
						AppendSummary (generatedComment, indent, out newCursorOffset);
					}
					
					Editor.InsertText (cursor, generatedComment.ToString ());
					Editor.CursorPosition = cursor + newCursorOffset;
					return true;
				}
				break;
			}
			return false;
		}
		
		//if this returns false, further handling of the key will not take place (including insertion into the editor)
		bool DoPreKeySmartIndent (Gdk.Key key)
		{			
			//handle some special cases
			switch (key) {
			case Gdk.Key.KP_Enter:
			case Gdk.Key.Return:
				if (Editor.SelectionEndPosition > Editor.SelectionStartPosition) {
					return true;
				}
				break;
			case Gdk.Key.Tab:
				if (Editor.SelectionEndPosition > Editor.SelectionStartPosition) {
					// user is conducting an "indent region"
					ResetSmartIndentEngineToCursor (Editor.SelectionStartPosition);
					return true;
				}
				break;
			}
			
			UpdateSmartIndentEngine ();
			
			//special handling to delete empty comment
			//DISABLED, makes it hard to escape from comments
			/*
			int cursor = Editor.CursorPosition;
			if (key == Gdk.Key.BackSpace) {
				bool emptyDocComment = indentEngine.IsInsideDocLineComment && Editor.GetLineText (indentEngine.LineNumber).Trim ().Length == 3;
				bool emptyMultilineComment = indentEngine.IsInsideMultiLineComment;
				if (emptyMultilineComment) {
					string line = Editor.GetLineText (indentEngine.LineNumber).Trim ();
					emptyMultilineComment = line.Length  <= 1 || line == "/*" || line == "/**";
				}
				if (emptyDocComment || emptyMultilineComment) {
					Editor.DeleteLine (indentEngine.LineNumber);
					Editor.InsertText (cursor, "\n");
				}
				return true;
			}
			*/
			return true;
		}
		
		//special handling for certain characters just inserted , for comments etc
		void DoPostInsertionSmartIndent (char charInserted, out bool reIndent)
		{
			UpdateSmartIndentEngine ();
			reIndent = false;
			int cursor = Editor.CursorPosition;
			char nextChar = Editor.GetCharAt (cursor);
			
			//System.Console.WriteLine ("char inserted: '{0}'", charInserted);
			//indentEngine.Debug ();
			
			switch (charInserted) {
			case '\n':
				if (indentEngine.LineNumber > 0) {
					string previousLine = Editor.GetLineText (indentEngine.LineNumber - 1);
					string trimmedPreviousLine = previousLine.TrimStart ();
					
					//xml doc comments
					if (trimmedPreviousLine.StartsWith ("/// ") && Editor.GetPositionFromLineColumn (indentEngine.LineNumber + 1, 1) > -1) {
						string nextLine = Editor.GetLineText (indentEngine.LineNumber + 1);
						if (nextLine.TrimStart ().StartsWith ("/// ")) {
						    Editor.InsertText (cursor, GetLineWhiteSpace (previousLine) + "/// ");
							return;
						}
					//multi-line comments
					} else if (indentEngine.IsInsideMultiLineComment) {
					    string commentPrefix = string.Empty;
						if (trimmedPreviousLine.StartsWith ("* ")) {
							commentPrefix = "* ";
						} else if (trimmedPreviousLine.StartsWith ("/**") || trimmedPreviousLine.StartsWith ("/*")) {
							commentPrefix = " * ";
						} else if (trimmedPreviousLine.StartsWith ("*")) {
							commentPrefix = "*";
						}
						Editor.InsertText (cursor, GetLineWhiteSpace (previousLine) + commentPrefix);
						return;
					}
				}
				//newline always reindents unless it's had special handling
				reIndent = true;
				break;
			case '\t':
				// Tab is a special case... depending on the context, the user may be
				// requesting a re-indent, tab-completing, or may just be wanting to
				// insert a literal tab.
				//
				// Tab is interpreted as a reindent command when it's neither at the end of a line nor in a verbatim string
				// and when a tab has just been inserted (i.e. not a template or an autocomplete command)
				if ((!indentEngine.IsInsideVerbatimString && (nextChar != '\n' || Editor.GetCharAt (cursor - 2) == '\n')) && Editor.GetCharAt (cursor -1) == '\t') {
					Editor.DeleteText (cursor - 1, 1);
					reIndent = true;
				}
				break;
			}
		}
		
		string GetLineWhiteSpace (string line)
		{
			int trimmedLength = line.TrimStart ().Length;
			return line.Substring (0, line.Length - trimmedLength);
		}
		
		//does re-indenting and cursor positioning
		void DoReSmartIndent ()
		{
			string newIndent = string.Empty;
			int cursor = Editor.CursorPosition;
			
			// Get context to the end of the line w/o changing the main engine's state
			CSharpIndentEngine ctx = (CSharpIndentEngine) indentEngine.Clone ();
			string line = Editor.GetLineText (ctx.LineNumber);
			for (int i = ctx.LineOffset; i < line.Length; i++) {
				ctx.Push (line[i]);
			}
			//System.Console.WriteLine("Re-indenting line '{0}'", line);
			
			// Measure the current indent
			int nlwsp = 0;
			while (nlwsp < line.Length && Char.IsWhiteSpace (line[nlwsp]))
				nlwsp++;
			
			int pos = Editor.GetPositionFromLineColumn (ctx.LineNumber, 1);
			string curIndent = line.Substring (0, nlwsp);
			int offset;
			
			if (cursor > pos + curIndent.Length)
				offset = cursor - (pos + curIndent.Length);
			else
				offset = 0;
			
			if (!indentEngine.LineBeganInsideMultiLineComment ||
			    (nlwsp < line.Length && line[nlwsp] == '*')) {
				// Possibly replace the indent
				newIndent = ctx.ThisLineIndent;
				
				if (newIndent != curIndent) {
					Editor.DeleteText (pos, nlwsp);
					Editor.InsertText (pos, newIndent);
					
					// Engine state is now invalid
					ResetSmartIndentEngineToCursor (pos);
				}
				
				pos += newIndent.Length;
			} else {
				pos += curIndent.Length;
			}
			
			pos += offset;
			if (pos != Editor.CursorPosition) {
				Editor.CursorPosition = pos;
				Editor.Select (pos, pos);
			}
		}
		
		public override IParameterDataProvider HandleParameterCompletion (ICodeCompletionContext completionContext, char completionChar)
		{
			if (completionChar == '(') {
				IParserContext pctx = GetParserContext ();
				int curPos = completionContext.TriggerOffset;
				
				// Get the text from the begining of the line
				int lin, col;
				Editor.GetLineColumnFromPosition (curPos, out lin, out col);
				string textToCursor = Editor.GetText (0, curPos - 1);
				
				// Find the expression before the '('
				ExpressionFinder expressionFinder = new ExpressionFinder (null);
				string ex = expressionFinder.FindExpression (textToCursor, textToCursor.Length - 1).Expression;
				if (ex == null)
					return null;
				
				// This is a bit of a hack, but for the resolver to properly resolve a constructor
				// call needs the new keyword and the brackets, so let's provide them
				int i = curPos - 2 - ex.Length;
				if (GetPreviousToken ("new", ref i, true))
					ex = "new " + ex + "()";
				
				// Find the language item at that position
				Resolver res = new Resolver (pctx);
				ILanguageItem it = res.ResolveIdentifier (pctx, ex, lin, col - 1, FileName, Editor.Text);
				
				MethodParameterDataProvider.Scope scope = MethodParameterDataProvider.Scope.All;
				if (it is IMember) {
					IMember member = it as IMember;
					IClass insideClass = LookupClass (res.CompilationUnit, lin, col);
					if (insideClass != null) {
						if (insideClass.FullyQualifiedName == member.DeclaringType.FullyQualifiedName) {
							scope = MethodParameterDataProvider.Scope.All;
						} else {
							scope = MethodParameterDataProvider.Scope.Public;
							foreach (IClass c in pctx.GetClassInheritanceTree (insideClass)) {
								if (c.FullyQualifiedName == member.DeclaringType.FullyQualifiedName) {
									scope |= MethodParameterDataProvider.Scope.Protected;
									break;
								}
							}
						}
						scope |= MethodParameterDataProvider.Scope.Internal;
					}
				}
				if (it is IMethod) {
					IMethod met = (IMethod) it;
					if (met.IsConstructor)
						return new CSharpParameterDataProvider (Editor, scope, met.DeclaringType);
					else
						return new CSharpParameterDataProvider (Editor, scope, met.DeclaringType, met.Name);
				}
				else if (it is IEvent) {
					IEvent ev = (IEvent) it;
					IClass cls = pctx.GetClass (ev.ReturnType.FullyQualifiedName, ev.ReturnType.GenericArguments, true, false);
					if (cls != null) {
						foreach(IMethod m in cls.Methods) {
							if (m.Name == "Invoke")
								return new CSharpParameterDataProvider (Editor, scope, cls, "Invoke");
						}
					}
				}
				else if (it is IClass) {
					return new CSharpParameterDataProvider (Editor, scope, (IClass)it);
				}
			}
			return null;
		}
		
		public override bool GetParameterCompletionCommandOffset (out int cpos)
		{
			cpos = Editor.CursorPosition - 1;
			while (cpos > 0) {
				char c = Editor.GetCharAt (cpos);
				if (c == '(') {
					int p = CSharpParameterDataProvider.GetCurrentParameterIndex (Editor, cpos + 1);
					if (p != -1) {
						cpos++;
						return true;
					}
				}
				cpos--;
			}
			return false;
		}

		
		bool IsInsideDocumentationComment (int cursor)
		{
			int lin, col;
			Editor.GetLineColumnFromPosition (cursor, out lin, out col);
			
			return Editor.GetLineText (lin).Trim ().StartsWith ("///");
		}
		
		public override ICompletionDataProvider HandleCodeCompletion (ICodeCompletionContext ctx, char charTyped)
		{
			if (charTyped == '#') {
				int lin, col;
				Editor.GetLineColumnFromPosition (Editor.CursorPosition, out lin, out col);
				if (col == 2)
					return GetDirectiveCompletionData ();
			}
			// Xml documentation code completion.
			if (charTyped == '<' && IsInsideDocumentationComment (Editor.CursorPosition)) 
				return GetXmlDocumentationCompletionData ();

			int caretLineNumber = ctx.TriggerLine + 1;
			int caretColumn = ctx.TriggerLineOffset + 1;

			ExpressionFinder expressionFinder = new ExpressionFinder (null);
			
			// Code completion of "new"
			
			int i = ctx.TriggerOffset;
			if (charTyped == ' ' && GetPreviousToken ("new", ref i, false)) {
				string token = GetPreviousToken (ref i, true);
				if (token == "=" || token == "throw") {
				
					IParserContext pctx = GetParserContext ();
					CodeCompletionDataProvider cp = new CodeCompletionDataProvider (pctx, GetAmbience ());
					
					Resolver res = new Resolver (pctx);
					
					IReturnType rt;
					string ex;
					caretColumn -= (i - ctx.TriggerOffset);
					
					if (token == "throw") {
						rt = new DefaultReturnType ("System.Exception");
						ex = "System.Exception";
					}
					else {
						ex = expressionFinder.FindExpression (Editor.GetText (0, i), i - 2).Expression;
						
						// Find the type of the variable that will hold the object
						rt = res.internalResolve (ex, caretLineNumber, caretColumn, FileName, Editor.Text);
						if (rt == null) {
							cp.Dispose ();
							return null;
						}
					}
					
					LanguageItemCollection items = res.IsAsResolve (ex, caretLineNumber, caretColumn, FileName, Editor.Text, true);
					TypeNameResolver resolver = res.CreateTypeNameResolver ();
					cp.AddResolveResults (items, true, resolver);
					
					// Add the variable type itself to the results list (IsAsResolve only returns subclasses)
					IClass cls = res.SearchType (rt, res.CompilationUnit);
					if (cls != null && cls.ClassType != ClassType.Interface && !cls.IsAbstract) {
						cp.AddResolveResult (cls, true, resolver);
						cp.DefaultCompletionString = GetAmbience ().Convert (cls, ConversionFlags.UseIntrinsicTypeNames, null);
					}
					
					return cp;
				}
			}
			//if (charTyped != '.' && charTyped != ' ')
			//	return null;
			
			// Completion of enum assignment
			i = ctx.TriggerOffset;
			if (charTyped == ' ' && GetPreviousToken ("=", ref i, true)) {
				IParserContext pctx = GetParserContext ();
				Resolver res = new Resolver (pctx);

				string ex = expressionFinder.FindExpression (Editor.GetText (0, i), i - 2).Expression;
				
				// Find the type of the variable that will hold the object
				IReturnType rt = res.internalResolve (ex, caretLineNumber, caretColumn, FileName, Editor.Text);
				if (rt != null) {
					IClass cls = res.SearchType (rt, res.CompilationUnit);
					if (cls != null && cls.ClassType == ClassType.Enum) {
						CodeCompletionDataProvider cp = new CodeCompletionDataProvider (pctx, GetAmbience ());
						TypeNameResolver resolver = res.CreateTypeNameResolver ();
						cp.AddResolveResult (cls, false, resolver);
						return cp;
					}
				}
				return null;
			}
			
			// Check for 'overridable' completion
			
			i = ctx.TriggerOffset;
			if (charTyped == ' ' && GetPreviousToken ("override", ref i, false)) {
				// Look for modifiers, in order to find the beginning of the declaration
				int firstMod = i;
				for (int n=0; n<3; n++) {
					string mod = GetPreviousToken (ref i, true);
					if (mod == "public" || mod == "protected" || mod == "private" || mod == "internal" || mod == "sealed") {
						firstMod = i;
					}
					else if (mod == "static") {
						// static methods are not overridable
						return null;
					}
					else
						break;
				}
				int line, column;
				Editor.GetLineColumnFromPosition (Editor.CursorPosition, out line, out column);
				
				IParserContext pctx = GetParserContext ();
				Resolver res = new Resolver (pctx);
				
				IClass cls = res.GetCallingClass (line, column, FileName, true);
				if (cls != null && (cls.ClassType == ClassType.Class || cls.ClassType == ClassType.Struct)) {
					string typedModifiers = Editor.GetText (firstMod, ctx.TriggerOffset);
					return GetOverridablesCompletionData (pctx, ctx, cls, firstMod, typedModifiers, res.CreateTypeNameResolver ());
				}
			}
			
			// Code completion of classes, members and namespaces
			
			//FindExpression call is *very* expensive, so try to avoid reaching it unless we have a handleable character
			if (charTyped == ' ' && ctx.TriggerOffset > 1) {
				char previousChar = Editor.GetCharAt (ctx.TriggerOffset - 2);
				if (char.IsWhiteSpace (previousChar))
					return null;
			} else if (charTyped != '(' && charTyped != '.') {
				return null;
			}
			
			string expression = expressionFinder.FindExpression (Editor.GetText (0, ctx.TriggerOffset), ctx.TriggerOffset - 2).Expression;
			if (expression == null)
				return null;
			IParserContext parserContext = GetParserContext ();
			CodeCompletionDataProvider completionProvider = new CodeCompletionDataProvider (parserContext, GetAmbience ());
			
			if (charTyped == '(') {
				if (expression.Trim () != "typeof")
					return null;
				string[] namespaces = parserContext.GetNamespaceList ("", true, true);
				Resolver res = new Resolver (parserContext);				
				LanguageItemCollection items = res.IsAsResolve ("System.Object", caretLineNumber, caretColumn, FileName, Editor.Text, false);
				TypeNameResolver resolver = res.CreateTypeNameResolver ();
				completionProvider.AddResolveResults (items, true, resolver);
				return completionProvider;
			}

			string ns;
			if (IsInUsing (expression, ctx.TriggerOffset, out ns)) {
				if (charTyped == ' ' && ns != String.Empty) {
					// 'using System' and charTyped == ' '
					// subnamespaces show up only on '.'
					return null;
				}
				
				Resolver res = new Resolver (parserContext);
				// Don't show namespaces when "using" is not a namespace directive
				IClass cls = res.GetCallingClass (caretLineNumber, caretColumn, FileName, false);
				if (cls != null)
					return null;
				string[] namespaces = parserContext.GetNamespaceList (ns, true, true);
				completionProvider.AddResolveResults (new ResolveResult(namespaces));
			} else if (charTyped == ' ') {
				if (expression == "is" || expression == "as") {
					string expr = expressionFinder.FindExpression (Editor.GetText (0, ctx.TriggerOffset), ctx.TriggerOffset - 5).Expression;
					Resolver res = new Resolver (parserContext);
					LanguageItemCollection items = res.IsAsResolve (expr, caretLineNumber, caretColumn, FileName, Editor.Text, false);
					completionProvider.AddResolveResults (items, true, res.CreateTypeNameResolver ());
				}
			} else {
				/* '.' */
				Resolver res = new Resolver (parserContext);
				ResolveResult results = res.Resolve (expression, caretLineNumber, caretColumn, FileName, Editor.Text);
				completionProvider.AddResolveResults (results, false, res.CreateTypeNameResolver ());
			}
			
			if (completionProvider.IsEmpty)
				return null;
			
			return completionProvider;
		}

		/* returns true in case
		 *	using  : ns - ""
		 *	using System. : ns - "System"
		 *	using System.Collections. : ns - "System.Collections"
		 */
		bool IsInUsing (string expr, int triggerOffset, out string ns)
		{
			int len = expr.Length;
			
			ns = String.Empty;
			if (expr == "using" || (expr.EndsWith ("using") && char.IsWhiteSpace (expr[len - 5])))
				return true;
			if (expr == "namespace" || (expr.EndsWith ("namespace") && char.IsWhiteSpace (expr[len - 9])))
				return true;
			
			ns = expr;
			int i = triggerOffset - expr.Length - 1;

			string token = GetPreviousToken (ref i, true);
			return (token == "using" || token == "namespace");
		}
		
		bool GetPreviousToken (string token, ref int i, bool allowLineChange)
		{
			return GetPreviousToken (ref i, allowLineChange) == token;
		}
		
		string GetPreviousToken (ref int i, bool allowLineChange)
		{
			char c;
			
			if (i <= 0)
				return null;
			
			do {
				c = Editor.GetCharAt (--i);
			} while (i > 0 && char.IsWhiteSpace (c) && (allowLineChange ? true : c != '\n'));
			
			if (i == 0)
				return null;
			
			if (!char.IsLetterOrDigit (c))
				return new string (c, 1);
			
			int endOffset = i + 1;
			
			do {
				c = Editor.GetCharAt (i - 1);
				if (!(char.IsLetterOrDigit (c) || c == '_'))
					break;
				
				i--;
			} while (i > 0);
			
			return Editor.GetText (i, endOffset);
		}
		
		ICompletionDataProvider GetOverridablesCompletionData (IParserContext pctx, ICodeCompletionContext ctx, IClass cls, int insertPos, string typedModifiers, ITypeNameResolver resolver)
		{
			ArrayList classMembers = new ArrayList ();
			ArrayList interfaceMembers = new ArrayList ();
			
			FindOverridables (pctx, cls, cls, classMembers, interfaceMembers);
			foreach (object mem in interfaceMembers)
				if (!classMembers.Contains (mem))
					classMembers.Add (mem);
			
			CSharpAmbience amb = new CSharpAmbience ();
			CodeCompletionDataProvider completionProvider = new CodeCompletionDataProvider (pctx, GetAmbience ());
			foreach (ILanguageItem mem in classMembers) {
				completionProvider.AddCompletionData (new OverrideCompletionData (Editor, mem, insertPos, typedModifiers, amb, resolver));
			}
			return completionProvider;
		}
		
		bool IsEqual (ParameterCollection c1, ParameterCollection c2)
		{
			if (c1.Count != c2.Count)
				return false;
			for (int i = 0; i < c1.Count; i++) {
				if (c1[i].ReturnType.FullyQualifiedName != c2[i].ReturnType.FullyQualifiedName)
					return false;
			}
			return true;
		}
		
		bool CanOverrideMethod (IClass cls, IMethod method)
		{
			foreach (IMethod m in cls.Methods) {
				if (method.Name == m.Name && IsEqual (method.Parameters, m.Parameters))
					return false;
			}
			return true;
		}
		
		bool CanOverrideProperty (IClass cls, IProperty prop)
		{
			foreach (IProperty p in cls.Properties) {
				if (prop.Name == p.Name)
					return false;
			}
			return true;
		}
		
		bool CanOverrideIndexer (IClass cls, IIndexer idx)
		{
			foreach (IProperty i in cls.Indexer) {
				if (idx.Name == i.Name && IsEqual (idx.Parameters, i.Parameters))
					return false;
			}
			return true;
		}
		
		void FindOverridables (IParserContext pctx, IClass motherClass, IClass cls, ArrayList classMembers, ArrayList interfaceMembers)
		{
			foreach (IReturnType rt in cls.BaseTypes)
			{
				if (cls.ClassType == ClassType.Interface)
					continue;

				IClass baseCls = pctx.GetClass (rt.FullyQualifiedName, rt.GenericArguments, true, true);
				if (baseCls == null)
					continue;

				bool isInterface = baseCls.ClassType == ClassType.Interface;
				if (isInterface && interfaceMembers == null)
					continue;
				ArrayList list = isInterface ? interfaceMembers : classMembers;
				
				foreach (IMethod m in baseCls.Methods) {
					if ((isInterface || m.IsVirtual || m.IsAbstract) && !m.IsSealed && CanOverrideMethod (motherClass, m))
						list.Add (m);
				}
				foreach (IProperty m in baseCls.Properties) {
					if ((isInterface || m.IsVirtual || m.IsAbstract) && !m.IsSealed && CanOverrideProperty (motherClass, m))
						list.Add (m);
				}
				foreach (IIndexer m in baseCls.Indexer) {
					if ((isInterface || m.IsVirtual || m.IsAbstract) && !m.IsSealed && CanOverrideIndexer (motherClass, m))
						list.Add (m);
				}
				foreach (IEvent m in baseCls.Events) {
					if ((isInterface || m.IsVirtual || m.IsAbstract) && !m.IsSealed)
						list.Add (m);
				}
				
				FindOverridables (pctx, motherClass, baseCls, classMembers, isInterface ? interfaceMembers : null);
			}
		}
		
		CodeCompletionDataProvider GetDirectiveCompletionData ()
		{
			CodeCompletionDataProvider cp = new CodeCompletionDataProvider (null, GetAmbience ());
			cp.AddCompletionData (new CodeCompletionData ("if", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("else", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("elif", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("endif", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("define", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("undef", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("warning", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("error", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("pragma", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("line", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("line hidden", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("line default", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("region", "md-literal"));
			cp.AddCompletionData (new CodeCompletionData ("endregion", "md-literal"));
			return cp;
		}
		static readonly List<string> commentTags = new List<string> (new string[] { "c", "code", "example", "exception", "include", "list", "listheader", "item", "term", "description", "para", "param", "paramref", "permission", "remarks", "returns", "see", "seealso", "summary", "value" });
		
		CodeCompletionDataProvider GetXmlDocumentationCompletionData ()
		{
			CodeCompletionDataProvider cp = new CodeCompletionDataProvider (null, GetAmbience ());
			cp.AddCompletionData (new CodeCompletionData ("c", "md-literal", GettextCatalog.GetString ("Marks text as code.")));
			cp.AddCompletionData (new CodeCompletionData ("code", "md-literal", GettextCatalog.GetString ("Marks text as code.")));
			cp.AddCompletionData (new CodeCompletionData ("example", "md-literal", GettextCatalog.GetString ("A description of the code sample.\nCommonly, this would involve use of the &lt;code&gt; tag.")));
			cp.AddCompletionData (new CodeCompletionData ("exception", "md-literal", GettextCatalog.GetString ("This tag lets you specify which exceptions can be thrown."), "exception cref=\"|\"></exception>"));
			cp.AddCompletionData (new CodeCompletionData ("include", "md-literal", GettextCatalog.GetString ("The &lt;include&gt; tag lets you refer to comments in another file that describe the types and members in your source code.\nThis is an alternative to placing documentation comments directly in your source code file."), "include file=\"|\" path=\"\">"));
			cp.AddCompletionData (new CodeCompletionData ("list", "md-literal", GettextCatalog.GetString ("Defines a list or table."), "list type=\"|\">"));
			cp.AddCompletionData (new CodeCompletionData ("listheader", "md-literal", GettextCatalog.GetString ("Defines a header for a list or table.")));
			cp.AddCompletionData (new CodeCompletionData ("item", "md-literal", GettextCatalog.GetString ("Defines an item for a list or table.")));
			cp.AddCompletionData (new CodeCompletionData ("term", "md-literal", GettextCatalog.GetString ("A term to define.")));
			cp.AddCompletionData (new CodeCompletionData ("description", "md-literal", GettextCatalog.GetString ("Describes a term in a list or table.")));
			cp.AddCompletionData (new CodeCompletionData ("para", "md-literal", GettextCatalog.GetString ("A text paragraph.")));

			cp.AddCompletionData (new CodeCompletionData ("param", "md-literal", GettextCatalog.GetString ("Describes a method parameter."), "param name=\"|\">"));
			cp.AddCompletionData (new CodeCompletionData ("paramref", "md-literal", GettextCatalog.GetString ("The &lt;paramref&gt; tag gives you a way to indicate that a word is a parameter."), "paramref name=\"|\"/>"));
			
			cp.AddCompletionData (new CodeCompletionData ("permission", "md-literal", GettextCatalog.GetString ("The &lt;permission&gt; tag lets you document the access of a member."), "permission cref=\"|\""));
			cp.AddCompletionData (new CodeCompletionData ("remarks", "md-literal", GettextCatalog.GetString ("The &lt;remarks&gt; tag is used to add information about a type, supplementing the information specified with &lt;summary&gt;.")));
			cp.AddCompletionData (new CodeCompletionData ("returns", "md-literal", GettextCatalog.GetString ("The &lt;returns&gt; tag should be used in the comment for a method declaration to describe the return value.")));
			cp.AddCompletionData (new CodeCompletionData ("see", "md-literal", GettextCatalog.GetString ("The &lt;see&gt; tag lets you specify a link from within text.")));
			cp.AddCompletionData (new CodeCompletionData ("seealso", "md-literal", GettextCatalog.GetString ("The &lt;seealso&gt; tag lets you specify the text that you might want to appear in a See Also section.")));
			cp.AddCompletionData (new CodeCompletionData ("summary", "md-literal", GettextCatalog.GetString ("The &lt;summary&gt; tag should be used to describe a type or a type member.")));
			cp.AddCompletionData (new CodeCompletionData ("value", "md-literal", GettextCatalog.GetString ("The &lt;value&gt; tag lets you describe a property.")));
			
			return cp;
		}
	}
	
}
