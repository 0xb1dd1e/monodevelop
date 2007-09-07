// CompletionTextEditorExtension.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (c) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//


using System;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Gui.Completion;
using MonoDevelop.Projects.Parser;
using MonoDevelop.Projects.Ambience;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Commands;

namespace MonoDevelop.Ide.Gui.Content
{
	public class CompletionTextEditorExtension: TextEditorExtension
	{
		CodeCompletionContext currentCompletionContext;
		ICompletionWidget completionWidget;
		bool autoHideCompletionWindow = true;
		
		// When a key is pressed, and before the key is processed by the editor, this method will be invoked.
		// Return true if the key press should be processed by the editor.
		public override bool KeyPress (Gdk.Key key, Gdk.ModifierType modifier)
		{
			bool res;
			
			if (currentCompletionContext != null) {
				autoHideCompletionWindow = false;
				if (CompletionListWindow.ProcessKeyEvent (key, modifier)) {
					autoHideCompletionWindow = true;
					return true;
				}
				autoHideCompletionWindow = false;
			}
			
			if (ParameterInformationWindowManager.IsWindowVisible) {
				if (ParameterInformationWindowManager.ProcessKeyEvent (key, modifier))
					return true;
				autoHideCompletionWindow = false;
			}
			
			res = base.KeyPress (key, modifier);

			// Handle code completion
			
			if (completionWidget != null && currentCompletionContext == null) {
				currentCompletionContext = completionWidget.CreateCodeCompletionContext (Editor.CursorPosition);
				ICompletionDataProvider cp = HandleCodeCompletion (currentCompletionContext, (char)(uint)key);
					
				if (cp != null)
					CompletionListWindow.ShowWindow ((char)(uint)key, cp, completionWidget, currentCompletionContext, OnCompletionWindowClosed);
				else
					currentCompletionContext = null;
			}
			
			// Handle parameter completion
			
			if (ParameterInformationWindowManager.IsWindowVisible)
				ParameterInformationWindowManager.PostProcessKeyEvent (key, modifier);

			if (completionWidget != null) {
				ICodeCompletionContext ctx = completionWidget.CreateCodeCompletionContext (Editor.CursorPosition);
				IParameterDataProvider paramProvider = HandleParameterCompletion (ctx, (char)(uint)key);
				if (paramProvider != null)
					ParameterInformationWindowManager.ShowWindow (ctx, paramProvider);
			}
			
			autoHideCompletionWindow = true;
			
			return res;
		}
		
		void OnCompletionWindowClosed ()
		{
			currentCompletionContext = null;
		}
		
		void OnCompletionContextChanged (object o, EventArgs a)
		{
			if (autoHideCompletionWindow) {
				if (currentCompletionContext != null)
					CompletionListWindow.HideWindow ();
				ParameterInformationWindowManager.HideWindow ();
			}
		}

		[CommandUpdateHandler (TextEditorCommands.ShowCompletionWindow)]
		internal void OnUpdateCompletionCommand (CommandInfo info)
		{
			info.Bypass = !CanRunCompletionCommand ();
		}
		
		[CommandHandler (TextEditorCommands.ShowCompletionWindow)]
		public virtual void RunCompletionCommand ()
		{
			ICompletionDataProvider cp = null;
			int cpos, wlen;
			if (!GetCompletionCommandOffset (out cpos, out wlen)) {
				cpos = Editor.CursorPosition;
				wlen = 0;
			}
			currentCompletionContext = completionWidget.CreateCodeCompletionContext (cpos);
			currentCompletionContext.TriggerWordLength = wlen;
			cp = CodeCompletionCommand (currentCompletionContext);
				
			if (cp != null)
				CompletionListWindow.ShowWindow ((char)0, cp, completionWidget, currentCompletionContext, OnCompletionWindowClosed);
			else
				currentCompletionContext = null;
		}
		
		public virtual bool CanRunCompletionCommand ()
		{
			return (completionWidget != null && currentCompletionContext == null);
		}
		
		public virtual ICompletionDataProvider HandleCodeCompletion (ICodeCompletionContext completionContext, char completionChar)
		{
			return null;
		}
		
		public virtual IParameterDataProvider HandleParameterCompletion (ICodeCompletionContext completionContext, char completionChar)
		{
			return null;
		}
		
		// return -1 if completion can't be shown
		public virtual bool GetCompletionCommandOffset (out int cpos, out int wlen)
		{
			cpos = wlen = 0;
			int pos = Editor.CursorPosition - 1;
			while (pos >= 0) {
				char c = Editor.GetCharAt (pos);
				if (!char.IsLetterOrDigit (c) && c != '_')
					break;
				pos--;
			}
			if (pos == -1)
				return false;
			
			pos++;
			cpos = pos;
			int len = Editor.TextLength;
			
			while (pos < len) {
				char c = Editor.GetCharAt (pos);
				if (!char.IsLetterOrDigit (c) && c != '_')
					break;
				pos++;
			}
			wlen = pos - cpos;
			return true;
		}

		public virtual ICompletionDataProvider CodeCompletionCommand (ICodeCompletionContext completionContext)
		{
			// This default implementation of CodeCompletionCommand calls HandleCodeCompletion providing
			// the char at the cursor position. If it returns a provider, just return it.
			
			int pos = completionContext.TriggerOffset;
			string txt = Editor.GetText (pos - 1, pos);
			if (txt.Length > 0) {
				ICompletionDataProvider cp = HandleCodeCompletion (completionContext, txt[0]);
				if (cp != null)
					return cp;
			}
			
			// If there is a parser context, try resolving by calling CtrlSpace.
			IParserContext ctx = GetParserContext();
			if (ctx != null) {
				CodeCompletionDataProvider completionProvider = new CodeCompletionDataProvider (ctx, GetAmbience ());
				completionProvider.AddResolveResults (ctx.CtrlSpace (completionContext.TriggerLine + 1, completionContext.TriggerLineOffset + 1, FileName));
				if (!completionProvider.IsEmpty)
					return completionProvider;
			}
			return null;
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
			
			completionWidget = Document.GetContent <ICompletionWidget> ();
			if (completionWidget != null)
				completionWidget.CompletionContextChanged += OnCompletionContextChanged;
		}
	}
}
