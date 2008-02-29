// SourceEditorView.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Gtk;
#if GNOME_PRINT
using Gnome;
#endif

using Mono.TextEditor;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Core;
using MonoDevelop.Projects.Gui.Completion;
using MonoDevelop.Projects.Parser;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui.Search;
using MonoDevelop.DesignerSupport.Toolbox;

namespace MonoDevelop.SourceEditor
{	
	public class SourceEditorView : AbstractViewContent, IPositionable, IExtensibleTextEditor, IBookmarkBuffer, IClipboardHandler, ICompletionWidget, IDocumentInformation, ICodeStyleOperations, ISplittable, IFoldable, IToolboxDynamicProvider, IToolboxConsumer, IZoomable
#if GNOME_PRINT
		, IPrintable
#endif
	{
		SourceEditorWidget widget;
		bool isDisposed = false;
		FileSystemWatcher fileSystemWatcher;
		static bool isInWrite = false;
		DateTime lastSaveTime;
		
		public Mono.TextEditor.Document Document {
			get {
				return widget.TextEditor.Document;
			}
		}
		
		public ExtendibleTextEditor TextEditor {
			get {
				return widget.TextEditor;
			}
		}
		
		public SourceEditorWidget SourceEditorWidget {
			get {
				return widget;
			}
		}
		
		public override Gtk.Widget Control {
			get {
				return widget;
			}
		}
		
		public SourceEditorView()
		{
			widget = new SourceEditorWidget (this);
			widget.TextEditor.Document.TextReplaced += delegate (object sender, ReplaceEventArgs args) {
				int startIndex = args.Offset;
				int endIndex   = startIndex + Math.Max (args.Count, args.Value != null ? args.Value.Length : 0);
				if (TextChanged != null)
					TextChanged (this, new TextChangedEventArgs (startIndex, endIndex));
			};
			
			widget.TextEditor.Document.TextReplaced += delegate {
				this.IsDirty = Document.IsDirty;
			};
//			widget.TextEditor.Document.DocumentUpdated += delegate {
//				this.IsDirty = Document.IsDirty;
//			};
			
			widget.TextEditor.Caret.PositionChanged += delegate {
				FireCompletionContextChanged ();
			};
			
//			GLib.Timeout.Add (1000, delegate {
//				if (!widget.IsSplitted)
//					widget.Split (true);
//				return true;
//			});
			
			widget.ShowAll ();
			
			fileSystemWatcher = new FileSystemWatcher ();
			fileSystemWatcher.Created += (FileSystemEventHandler)MonoDevelop.Core.Gui.DispatchService.GuiDispatch (new FileSystemEventHandler (OnFileChanged));	
			fileSystemWatcher.Changed += (FileSystemEventHandler)MonoDevelop.Core.Gui.DispatchService.GuiDispatch (new FileSystemEventHandler (OnFileChanged));
			
			this.ContentNameChanged += delegate {
				this.Document.FileName   = this.ContentName;
				isInWrite = true;
				if (String.IsNullOrEmpty (ContentName) || !File.Exists (ContentName))
					return;
				
				fileSystemWatcher.EnableRaisingEvents = false;
				lastSaveTime = File.GetLastWriteTime (ContentName);
				fileSystemWatcher.Path = Path.GetDirectoryName (ContentName);
				fileSystemWatcher.Filter = Path.GetFileName (ContentName);
				isInWrite = false;
				fileSystemWatcher.EnableRaisingEvents = true;
			};
			ClipbardRingUpdated += UpdateClipboardRing;
		}
		
		public override void Save (string fileName)
		{
			Save (fileName, null);
		}
		
		public void Save (string fileName, string encoding)
		{
			fileName = ConvertFileNameToVFS (fileName);

			if (warnOverwrite) {
				if (fileName == ContentName) {
					if (!MonoDevelop.Core.Gui.Services.MessageService.AskQuestion (string.Format (GettextCatalog.GetString ("This file {0} has been changed outside of MonoDevelop. Are you sure you want to overwrite the file?"), fileName),"MonoDevelop"))
						return;
				}
				warnOverwrite = false;
				widget.RemoveReloadBar ();
				WorkbenchWindow.ShowNotification = false;
			}
			
			isInWrite = true;
			try {
				File.WriteAllText (fileName, Document.Text);
				lastSaveTime = File.GetLastWriteTime (fileName);
			} finally {
				isInWrite = false;
			}
				
//			if (encoding != null)
//				se.Buffer.SourceEncoding = encoding;
//			TextFileService.FireCommitCountChanges (this);
			
			ContentName = fileName; 
			Document.MimeType = IdeApp.Services.PlatformService.GetMimeTypeForUri (fileName);
			Document.SetNotDirtyState ();
			this.IsDirty = false;
		}
		
		public override void Load (string fileName)
		{
			Load (fileName, null);
		}
		
		
		static string ConvertFileNameToVFS (string fileName)
		{
			string result = fileName;
			result = result.Replace ("%", "%25");
			result = result.Replace ("#", "%23");
			result = result.Replace ("?", "%3F");
			return result;
		}
		
		bool warnOverwrite = false;
		public void Load (string fileName, string encoding)
		{
			fileName = ConvertFileNameToVFS (fileName);
			if (warnOverwrite) {
				warnOverwrite = false;
				widget.RemoveReloadBar ();
				WorkbenchWindow.ShowNotification = false;
			}
			
			Document.MimeType = IdeApp.Services.PlatformService.GetMimeTypeForUri (fileName);
			Document.Text = File.ReadAllText (fileName);
			ContentName = fileName;
//			InitializeFormatter ();
//			
//			if (Services.DebuggingService != null) {
//				foreach (IBreakpoint b in Services.DebuggingService.GetBreakpointsAtFile (fileName))
//					se.View.ShowBreakpointAt (b.Line - 1);
//					
//				UpdateExecutionLocation ();
//			}
//			
			widget.LoadClassCombo ();
			this.IsDirty = false;
		}
		
		public override void Dispose()
		{
			this.isDisposed= true;
			ClipbardRingUpdated -= UpdateClipboardRing;
			if (fileSystemWatcher != null) {
				fileSystemWatcher.EnableRaisingEvents = false;
				fileSystemWatcher.Dispose ();
				fileSystemWatcher = null;
			}
			
			if (widget != null) {
				widget.Destroy ();
				widget.Dispose ();
				widget = null;
			}
		}
		
		public IParserContext GetParserContext ()
		{
			IParserDatabase pdb = IdeApp.ProjectOperations.ParserDatabase;
			
			Project project = Project;
			if (project != null) 
				return pdb.GetProjectParserContext (project);
			
			return pdb.GetFileParserContext (IsUntitled ? UntitledName : ContentName);
		}
		
		public MonoDevelop.Projects.Ambience.Ambience GetAmbience ()
		{
			Project project = Project;
			if (project != null)
				return project.Ambience;
			string file = this.IsUntitled ? this.UntitledName : this.ContentName;
			return MonoDevelop.Projects.Services.Ambience.GetAmbienceForFile (file);
		}
		
		void OnFileChanged (object sender, FileSystemEventArgs args)
		{
			if (!isInWrite && args.FullPath != ContentName)
				return;
			if (lastSaveTime == File.GetLastWriteTime (ContentName))
				return;
			
			if (args.ChangeType == WatcherChangeTypes.Changed || args.ChangeType == WatcherChangeTypes.Created) 
				widget.ShowFileChangedWarning ();
		}
		
		#region IExtensibleTextEditor
		ITextEditorExtension IExtensibleTextEditor.AttachExtension (ITextEditorExtension extension)
		{
			this.widget.TextEditor.Extension = extension;
			return this.widget;
		}
		
//		protected override void OnMoveCursor (MovementStep step, int count, bool extend_selection)
//		{
//			base.OnMoveCursor (step, count, extend_selection);
//			if (extension != null)
//				extension.CursorPositionChanged ();
//		}
		
//		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
//		{
//			if (extension != null)
//				return extension.KeyPress (evnt.Key, evnt.State);
//			return this.KeyPress (evnt.Key, evnt.State); 
//		}		
		#endregion
		
		#region IEditableTextBuffer
		public IClipboardHandler ClipboardHandler {
			get {
				return this;
			}
		}
		
		public bool EnableUndo {
			get {
				return this.Document.CanUndo && widget.EditorHasFocus;
			}
		}
		
		public void Undo()
		{
			this.Document.Undo ();
		}
		
		public bool EnableRedo {
			get {
				return this.Document.CanRedo && widget.EditorHasFocus;
			}
		}
		
		public void Redo()
		{
			this.Document.Redo ();
		}
		
		public void BeginAtomicUndo ()
		{
			this.Document.BeginAtomicUndo ();
		}
		public void EndAtomicUndo ()
		{
			this.Document.EndAtomicUndo ();
		}
			
		public string SelectedText { 
			get {
				return TextEditor.IsSomethingSelected ? Document.GetTextAt (TextEditor.SelectionRange) : "";
			}
			set {
				TextEditor.DeleteSelectedText ();
				Document.Insert (TextEditor.Caret.Offset, new StringBuilder (value));
				TextEditor.SelectionRange = new Segment (TextEditor.Caret.Offset, value.Length);
				TextEditor.Caret.Offset += value.Length; 
			}
		}
		
		public event TextChangedEventHandler TextChanged;
		#endregion
		
		#region ITextBuffer
		public int CursorPosition { 
			get {
				return TextEditor.Caret.Offset;
			}
			set {
				TextEditor.Caret.Offset = value;
			}
		}

		public int SelectionStartPosition { 
			get {
				if (!TextEditor.IsSomethingSelected)
					return TextEditor.Caret.Offset;
				return TextEditor.SelectionRange.Offset;
			}
		}
		public int SelectionEndPosition { 
			get {
				if (!TextEditor.IsSomethingSelected)
					return TextEditor.Caret.Offset;
				return TextEditor.SelectionRange.EndOffset;
			}
		}
		
		public void Select (int startPosition, int endPosition)
		{
			TextEditor.SelectionRange = new Segment (startPosition, endPosition - startPosition);
			TextEditor.ScrollToCaret ();
		}
		
		public void ShowPosition (int position)
		{
			// TODO
		}
		#endregion
		
		#region ITextFile
		public string Name {
			get { 
				return this.ContentName; 
			} 
		}

		public string Text {
			get {
				return this.widget.TextEditor.Document.Text;
			}
			set {
				this.widget.TextEditor.Document.Text = value;
			}
		}
		
		public int Length { 
			get {
				return this.widget.TextEditor.Document.Length;
			}
		}

		public bool WarnOverwrite {
			get {
				return warnOverwrite;
			}
			set {
				warnOverwrite = value;
			}
		}

		public string GetText (int startPosition, int endPosition)
		{
			if (startPosition < 0 || endPosition < 0 || startPosition > endPosition)
				return "";
			return this.widget.TextEditor.Document.GetTextAt (startPosition, endPosition - startPosition);
		}
		
		public char GetCharAt (int position)
		{
			return this.widget.TextEditor.Document.GetCharAt (position);
		}
		
		public int GetPositionFromLineColumn (int line, int column)
		{
			return this.widget.TextEditor.Document.LocationToOffset (new DocumentLocation (line - 1, column - 1));
		}
		public void GetLineColumnFromPosition (int position, out int line, out int column)
		{
			DocumentLocation location = this.widget.TextEditor.Document.OffsetToLocation (position);
			line   = location.Line + 1;
			column = location.Column + 1;
		}
		#endregion
		
		#region IEditableTextFile
		public void InsertText (int position, string text)
		{
			this.widget.TextEditor.Document.Insert (position, new StringBuilder (text));
			if (text != null && this.widget.TextEditor.Caret.Offset >= position) 
				this.widget.TextEditor.Caret.Offset += text.Length;
		}
		public void DeleteText (int position, int length)
		{
			this.widget.TextEditor.Document.Remove (position, length);
			if (this.widget.TextEditor.Caret.Offset >= position) 
				this.widget.TextEditor.Caret.Offset -= length;
		}
		#endregion 
		
		#region IPositionable
		public void JumpTo (int line, int column)
		{
			
			GLib.Timeout.Add (20,  delegate {
				if (this.isDisposed)
					return false;
				line = Math.Min (line, Document.LineCount);
				
				widget.TextEditor.Caret.Location = new DocumentLocation (line - 1, column - 1);
				
				widget.TextEditor.GrabFocus ();
				widget.TextEditor.ScrollToCaret ();
				return false;
			});
		}
		#endregion
		
		#region IBookmarkBuffer
		LineSegment GetLine (int position)
		{
			DocumentLocation location = Document.OffsetToLocation (position);
			return Document.GetLine (location.Line);
		}
				
		public void SetBookmarked (int position, bool mark)
		{
			LineSegment line = GetLine (position);
			if (line != null && line.IsBookmarked != mark) {
				int lineNumber = widget.TextEditor.Document.OffsetToLineNumber (line.Offset);
				line.IsBookmarked = mark;
				widget.TextEditor.Document.RequestUpdate (new LineUpdate (lineNumber));
				widget.TextEditor.Document.CommitDocumentUpdate ();
			}
		}
		
		public bool IsBookmarked (int position)
		{
			LineSegment line = GetLine (position);
			return line != null ? line.IsBookmarked : false;
		}
		
		public void PrevBookmark ()
		{
			TextEditor.RunAction (new GotoPrevBookmark ());
		}
		
		public void NextBookmark ()
		{
			TextEditor.RunAction (new GotoNextBookmark ());
		}
		public void ClearBookmarks ()
		{
			TextEditor.RunAction (new ClearAllBookmarks ());
		}
		#endregion
		
		#region IClipboardHandler
		public bool EnableCut {
			get {
				return widget.EditorHasFocus && TextEditor.IsSomethingSelected;
			}
		}
		public bool EnableCopy {
			get {
				return EnableCut;
			}
		}
		public bool EnablePaste {
			get {
				return widget.EditorHasFocus;
			}
		}
		public bool EnableDelete {
			get {
				return widget.EditorHasFocus && TextEditor.IsSomethingSelected;
			}
		}
		public bool EnableSelectAll {
			get {
				return widget.EditorHasFocus;
			}
		}
		
		public void Cut (object sender, EventArgs args)
		{
			TextEditor.RunAction (new CutAction ());
		}
		
		public void Copy (object sender, EventArgs args)
		{
			TextEditor.RunAction (new CopyAction ());
		}
		
		public void Paste (object sender, EventArgs args)
		{
			TextEditor.RunAction (new PasteAction ());
		}
		
		public void Delete (object sender, EventArgs args)
		{
			if (TextEditor.IsSomethingSelected) 
				TextEditor.DeleteSelectedText ();
		}
		
		public void SelectAll (object sender, EventArgs args)
		{
			TextEditor.RunAction (new SelectionSelectAll ());
		}
		#endregion
		
		#region ICompletionWidget
		public int TextLength {
			get {
				return Document.Length;
			}
		}
		public int SelectedLength { 
			get {
				return TextEditor.IsSomethingSelected ? TextEditor.SelectionRange.Length : 0;
			}
		}
//		public string GetText (int startOffset, int endOffset)
//		{
//			return this.widget.TextEditor.Document.Buffer.GetTextAt (startOffset, endOffset - startOffset);
//		}
		public char GetChar (int offset)
		{
			return Document.GetCharAt (offset);
		}
		
		public Gtk.Style GtkStyle { 
			get {
				return widget.Style.Copy ();
			}
		}

		public CodeCompletionContext CreateCodeCompletionContext (int triggerOffset) 
		{
			CodeCompletionContext result = new CodeCompletionContext ();
			result.TriggerOffset = triggerOffset;
			DocumentLocation loc = Document.OffsetToLocation (triggerOffset);
			result.TriggerLine   = loc.Line;
			result.TriggerLineOffset = loc.Column;
			Gdk.Point p = this.widget.TextEditor.DocumentToVisualLocation (loc);
			int tx, ty;
			
			widget.ParentWindow.GetOrigin (out tx, out ty);
			tx += TextEditor.Allocation.X;
			ty += TextEditor.Allocation.Y;
			result.TriggerXCoord = tx + p.X + TextEditor.TextViewMargin.XOffset - (int)TextEditor.HAdjustment.Value;
			result.TriggerYCoord = ty + p.Y - (int)TextEditor.VAdjustment.Value + TextEditor.LineHeight;
			result.TriggerTextHeight = TextEditor.LineHeight;
			return result;
		}
		
		public string GetCompletionText (ICodeCompletionContext ctx)
		{
			if (ctx == null)
				return null;
			int min = Math.Min (ctx.TriggerOffset, TextEditor.Caret.Offset);
			int max = Math.Max (ctx.TriggerOffset, TextEditor.Caret.Offset);
			return Document.GetTextBetween (min, max);
		}
		
		public void SetCompletionText (ICodeCompletionContext ctx, string partial_word, string complete_word)
		{
			TextEditor.DeleteSelectedText ();
			
			int idx = complete_word.IndexOf ('|'); // | in the completion text now marks the caret position
			if (idx >= 0) {
				complete_word = complete_word.Remove (idx, 1);
			} else {
				idx = complete_word.Length;
			}
			
			this.widget.TextEditor.Document.Replace (ctx.TriggerOffset, String.IsNullOrEmpty (partial_word) ? 0 : partial_word.Length, new StringBuilder (complete_word));
			this.widget.TextEditor.Caret.Offset = ctx.TriggerOffset + idx;
		}
		
		void FireCompletionContextChanged ()
		{
			if (CompletionContextChanged != null)
				CompletionContextChanged (this, EventArgs.Empty);
		}
		
		public event EventHandler CompletionContextChanged;
		#endregion
		
		#region IDocumentInformation
		string IDocumentInformation.FileName {
			get { 
				return this.IsUntitled ? UntitledName : ContentName; 
			}
		}
		
		public ITextIterator GetTextIterator ()
		{
			return new DocumentTextIterator (this, TextEditor.Caret.Offset);
		}
		
		public string GetLineTextAtOffset (int offset)
		{
			LineSegment line = this.widget.TextEditor.Document.GetLineByOffset (offset);
			if (line == null)
				return null;
			return this.widget.TextEditor.Document.GetTextAt (line);
		}
		
		class DocumentTextIterator : ITextIterator
		{
			SourceEditorView view;
			int initialOffset, offset;
			
			public DocumentTextIterator (SourceEditorView view, int offset)
			{
				this.view = view;
				this.initialOffset = this.offset = offset;
			}
			
			public char Current {
				get {
					return view.Document.GetCharAt (offset);
				}
			}
			public int Position {
				get {
					return offset;
				}
				set {
					offset = value;
				}
			}
			public int Line { 
				get {
					return view.Document.OffsetToLineNumber (offset);
				}
			}
			
			public int Column {
				get {
					return view.Document.OffsetToLocation (offset).Column;
				}
			}
			
			public int DocumentOffset { 
				get {
					return offset;
				}
			}
			
			public char GetCharRelative (int offset)
			{
				return view.Document.GetCharAt (this.offset + offset);
			}
			
			public bool MoveAhead (int numChars)
			{
				bool result = offset + numChars < view.Document.Length && offset + numChars >= 0;
				offset = Math.Max (Math.Min (offset + numChars, view.Document.Length - 1), 0);
				return result;
			}
			public void MoveToEnd ()
			{
				offset = view.Document.Length - 1;
			}
			public string ReadToEnd ()
			{
				return view.Document.GetTextAt (offset, view.Document.Length - offset);
			}
					
			public void Reset()
			{
				offset = this.initialOffset;
			}
			public void Replace (int length, string pattern)
			{
				view.Document.Replace (offset, length, new StringBuilder (pattern));
			}
			public void Close ()
			{
				// nothing
			}
		
			public IDocumentInformation DocumentInformation { 
				get {
					return this.view;
				}
			}
			
			public bool SupportsSearch (MonoDevelop.Ide.Gui.Search.SearchOptions options, bool reverse)
			{
				return false;
			}
			public bool SearchNext (string text, MonoDevelop.Ide.Gui.Search.SearchOptions options, bool reverse)
			{
				return false;
			}
		}
		#endregion
		
		#region ICodeStyleOperations
		public void ToggleCodeComment ()
		{
			bool comment = false;
			string commentTag = Services.Languages.GetBindingPerFileName (this.ContentName).CommentTag ?? "//";
			foreach (LineSegment line in TextEditor.SelectedLines) {
				string text = Document.GetTextAt (line);
				string trimmedText = text.TrimStart ();
				if (!trimmedText.StartsWith (commentTag)) {
					comment = true;
					break;
				}
			}
			if (comment) {
				CommentCode ();
			} else {
				UncommentCode ();
			}
		}
		
		public void CommentCode ()
		{
			int startLineNr = TextEditor.IsSomethingSelected ? Document.OffsetToLineNumber (TextEditor.SelectionRange.Offset) : TextEditor.Caret.Line;
			int endLineNr   = TextEditor.IsSomethingSelected ? Document.OffsetToLineNumber (TextEditor.SelectionRange.EndOffset) : TextEditor.Caret.Line;
			if (endLineNr < 0)
				endLineNr = Document.LineCount;
			
			LineSegment anchorLine   = TextEditor.IsSomethingSelected ? TextEditor.Document.GetLineByOffset (TextEditor.SelectionAnchor) : null;
			int         anchorColumn = TextEditor.IsSomethingSelected ? TextEditor.SelectionAnchor - anchorLine.Offset : -1;
			
			StringBuilder commentTag = new StringBuilder(Services.Languages.GetBindingPerFileName (this.ContentName).CommentTag ?? "//");
			Document.BeginAtomicUndo ();
			foreach (LineSegment line in TextEditor.SelectedLines) {
				Document.Insert (line.Offset, commentTag);
			}
			if (TextEditor.IsSomethingSelected) {
				if (TextEditor.SelectionAnchor < TextEditor.Caret.Offset) {
					if (anchorColumn != 0) 
						TextEditor.SelectionAnchor = System.Math.Min (anchorLine.Offset + anchorLine.EditableLength, System.Math.Max (anchorLine.Offset, TextEditor.SelectionAnchor + commentTag.Length));
				} else {
					if (anchorColumn != 0) {
						TextEditor.SelectionAnchor = System.Math.Min (anchorLine.Offset + anchorLine.EditableLength, System.Math.Max (anchorLine.Offset, anchorLine.Offset + anchorColumn + commentTag.Length));
					} else {
						TextEditor.SelectionAnchor = anchorLine.Offset;
					}
				}
			}
			
			if (TextEditor.Caret.Column != 0) {
				TextEditor.Caret.PreserveSelection = true;
				TextEditor.Caret.Column += commentTag.Length;
				TextEditor.Caret.PreserveSelection = false;
			}
			
			if (TextEditor.IsSomethingSelected) 
				TextEditor.ExtendSelectionTo (TextEditor.Caret.Offset);
			Document.EndAtomicUndo ();
			Document.CommitMultipleLineUpdate (startLineNr, endLineNr);
		}
		
		public void UncommentCode ()
		{
			int startLineNr = TextEditor.IsSomethingSelected ? Document.OffsetToLineNumber (TextEditor.SelectionRange.Offset) : TextEditor.Caret.Line;
			int endLineNr   = TextEditor.IsSomethingSelected ? Document.OffsetToLineNumber (TextEditor.SelectionRange.EndOffset) : TextEditor.Caret.Line;
			if (endLineNr < 0)
				endLineNr = Document.LineCount;
			LineSegment anchorLine   = TextEditor.IsSomethingSelected ? TextEditor.Document.GetLineByOffset (TextEditor.SelectionAnchor) : null;
			int         anchorColumn = TextEditor.IsSomethingSelected ? TextEditor.SelectionAnchor - anchorLine.Offset : -1;
			
			string commentTag = Services.Languages.GetBindingPerFileName (this.ContentName).CommentTag ?? "//";
			Document.BeginAtomicUndo ();
			int first = -1;
			int last  = 0;
			foreach (LineSegment line in TextEditor.SelectedLines) {
				string text = Document.GetTextAt (line);
				string trimmedText = text.TrimStart ();
				int length = 0;
				if (trimmedText.StartsWith (commentTag)) {
					Document.Remove (line.Offset + (text.Length - trimmedText.Length), commentTag.Length);
					length = commentTag.Length;
				}
				last = length;
				if (first < 0)
					first = last;
			}
			
			if (TextEditor.IsSomethingSelected) {
				if (TextEditor.SelectionAnchor < TextEditor.Caret.Offset) {
					TextEditor.SelectionAnchor = System.Math.Min (anchorLine.Offset + anchorLine.EditableLength, System.Math.Max (anchorLine.Offset, TextEditor.SelectionAnchor - first));
				} else {
					TextEditor.SelectionAnchor = System.Math.Min (anchorLine.Offset + anchorLine.EditableLength, System.Math.Max (anchorLine.Offset, anchorLine.Offset + anchorColumn - last));
				}
			}
			
			if (TextEditor.Caret.Column != 0) {
				TextEditor.Caret.PreserveSelection = true;
				TextEditor.Caret.Column = System.Math.Max (0, TextEditor.Caret.Column - last);
				TextEditor.Caret.PreserveSelection = false;
			}
			
			if (TextEditor.IsSomethingSelected) 
				TextEditor.ExtendSelectionTo (TextEditor.Caret.Offset);
		
			Document.EndAtomicUndo ();
			Document.CommitMultipleLineUpdate (startLineNr, endLineNr);
		}
		
		public void IndentSelection ()
		{
			InsertTab.IndentSelection (TextEditor.GetTextEditorData ());
		}
		
		public void UnIndentSelection ()
		{
			RemoveTab.RemoveIndentSelection (TextEditor.GetTextEditorData ());
		}
		#endregion
		
		#region ISplittable
		public bool EnableSplitHorizontally {
			get {
				return !EnableUnsplit;
			}
		}
		public bool EnableSplitVertically {
			get {
				return !EnableUnsplit;
			}
		}
		public bool EnableUnsplit {
			get {
				return widget.IsSplitted;
			}
		}
		
		public void SplitHorizontally ()
		{
			widget.Split (false);
		}
		
		public void SplitVertically ()
		{
			widget.Split (true);
		}
		
		public void Unsplit ()
		{
			widget.Unsplit ();
		}
		
		public void SwitchWindow ()
		{
			widget.SwitchWindow ();
		}
		
		#endregion
		
		#region IFoldable
		void ToggleFoldings (ICollection<FoldSegment> segments)
		{
			bool doFold = true;
			foreach (FoldSegment segment in segments) {
				if (segment.IsFolded) {
					doFold = false;
					break;
				}
			}
			foreach (FoldSegment segment in segments) {
				segment.IsFolded = doFold;
			}
			Document.RequestUpdate (new UpdateAll ());
			Document.CommitDocumentUpdate ();
		}
		
		public void ToggleAllFoldings ()
		{
			ToggleFoldings (Document.FoldSegments);
		}
		
		public void FoldDefinitions ()
		{
			foreach (FoldSegment segment in Document.FoldSegments) {
				segment.IsFolded = segment.FoldingType == FoldingType.TypeMember;
			}
			Document.RequestUpdate (new UpdateAll ());
			Document.CommitDocumentUpdate ();
		}
		
		public void ToggleFolding ()
		{
			ToggleFoldings (Document.GetStartFoldings (Document.GetLine (TextEditor.Caret.Line)));
		}
		#endregion
		
#if GNOME_PRINT
		#region IPrintable
		PrintDialog    printDialog;
		Gnome.PrintJob printJob;
		
		public void PrintDocument ()
		{
			if (printDialog != null) 
				return;
			CreatePrintJob ();
			
			printDialog = new PrintDialog (printJob, GettextCatalog.GetString ("Print Source Code"));
			printDialog.SkipTaskbarHint = true;
			printDialog.Modal = true;
//			printDialog.IconName = "gtk-print";
			printDialog.SetPosition (WindowPosition.CenterOnParent);
			printDialog.Gravity = Gdk.Gravity.Center;
			printDialog.TypeHint = Gdk.WindowTypeHint.Dialog;
			printDialog.TransientFor = IdeApp.Workbench.RootWindow;
			printDialog.KeepAbove = false;
			printDialog.Response += OnPrintDialogResponse;
			printDialog.Close += delegate {
				printDialog = null;
			};
			printDialog.Run ();
		}
		
		public void PrintPreviewDocument ()
		{
			CreatePrintJob ();
			PrintJobPreview preview = new PrintJobPreview (printJob, GettextCatalog.GetString ("Print Preview - Source Code"));
			preview.Modal = true;
			preview.SetPosition (WindowPosition.CenterOnParent);
			preview.Gravity = Gdk.Gravity.Center;
			preview.TransientFor = printDialog != null ? printDialog : IdeApp.Workbench.RootWindow;
//			preview.IconName = "gtk-print-preview";
			preview.ShowAll ();
		}
		
		void OnPrintDialogResponse (object sender, Gtk.ResponseArgs args)
		{
			switch ((int)args.ResponseId) {
			case (int)PrintButtons.Print:
				int result = printJob.Print ();
				if (result != 0)
					IdeApp.Services.MessageService.ShowError (GettextCatalog.GetString ("Print operation failed."));
				goto default;
			case (int)PrintButtons.Preview:
				PrintPreviewDocument ();
				break;
			default:
				printDialog.HideAll ();
				printDialog.Destroy ();
				break;
			}
		}
		
		const int marginTop    = 50;
		const int marginBottom = 50;
		const int marginLeft   = 30;
		const int marginRight  = 30;
		
		int yPos = 0;
		int xPos = 0;
		int page = 0;
		int totalPages = 0;
		
		double pageWidth, pageHeight;
		
		void PrintHeader (Gnome.PrintContext gpc, Gnome.PrintConfig config)
		{
			gpc.SetRgbColor (0, 0, 0);
			string header = GettextCatalog.GetString ("File:") +  " " + StrMiddleTruncate (IdeApp.Workbench.ActiveDocument.FileName, 60);
			yPos = marginTop;
			gpc.MoveTo (xPos, pageHeight - yPos);
			gpc.Show (header);
			xPos = marginLeft;
			gpc.RectFilled (marginLeft, pageHeight - (marginTop + 5), pageWidth - marginRight - marginLeft, 2);
			yPos += widget.TextEditor.LineHeight;
		}
		
		void PrintFooter (Gnome.PrintContext gpc, Gnome.PrintConfig config)
		{
			gpc.SetRgbColor (0, 0, 0);
			gpc.MoveTo (xPos, marginBottom);
			gpc.Show ("MonoDevelop");
			gpc.MoveTo (xPos + 200, marginBottom);
			string footer = GettextCatalog.GetString ("Page") + " " + page + "/" + (totalPages + 1);
			gpc.Show (footer);
			gpc.RectFilled (marginLeft, marginBottom - 3 + widget.TextEditor.LineHeight, pageWidth - marginRight - marginLeft, 2);
		}
		
		void MyPrint (Gnome.PrintContext gpc, Gnome.PrintConfig config)
		{
			config.GetPageSize (out pageWidth, out pageHeight);
			int linesPerPage = (int)((pageHeight - marginBottom - marginTop - 10) / widget.TextEditor.LineHeight);
			linesPerPage -= 2;
			totalPages = Document.LineCount / linesPerPage;
			xPos = marginLeft;
			string fontName = SourceEditorOptions.Options.FontName;
			Gnome.Font font =  Gnome.Font.FindClosestFromFullName (fontName);
			if (font == null) {
				LoggingService.LogError ("Can't find font: '" + fontName + "', trying default." );
				font = Gnome.Font.FindClosestFromFullName (IdeApp.Services.PlatformService.DefaultMonospaceFont);
			}
			if (font == null) {
				LoggingService.LogError ("Unable to load font." );
				MonoDevelop.Core.Gui.Services.MessageService.ShowError ("Unable to initialize Font, aborting.");
				return;
			}
			Gnome.Font boldFont   =  Gnome.Font.FindFromFullName (font.FontName + " Bold " + ((int)font.Size));
			Gnome.Font italicFont =  Gnome.Font.FindFromFullName (font.FontName + " Italic " + ((int)font.Size));
			
			gpc.BeginPage ("page " + page++);
			PrintHeader (gpc, config);
			foreach (LineSegment line in Document.Lines) {
				if (yPos >= pageHeight - marginBottom - 5 - widget.TextEditor.LineHeight) {
					gpc.SetFont (font);
					yPos = marginTop;
					PrintFooter (gpc, config);
					gpc.ShowPage ();
					gpc.BeginPage ("page " + page++);
					PrintHeader (gpc, config);
				}
				Chunk[] chunks = Document.SyntaxMode.GetChunks (Document, TextEditor.ColorStyle, line, line.Offset, line.Length);
				foreach (Chunk chunk in chunks) {
					string text = Document.GetTextAt (chunk);
					text = text.Replace ("\t", new string (' ', TextEditorOptions.Options.TabSize));
					gpc.SetRgbColor (chunk.Style.Color.Red / (double)ushort.MaxValue, 
					                 chunk.Style.Color.Green / (double)ushort.MaxValue, 
					                 chunk.Style.Color.Blue / (double)ushort.MaxValue);
					
					gpc.MoveTo (xPos, pageHeight - yPos);
					if (chunk.Style.Bold) {
						gpc.SetFont (boldFont);
					} else if (chunk.Style.Italic) {
						gpc.SetFont (italicFont);
					} else {
						gpc.SetFont (font);
					}
					gpc.Show (text);
					xPos += widget.TextEditor.TextViewMargin.GetWidth (text);
				}
				xPos = marginLeft;
				yPos += widget.TextEditor.LineHeight;
			}
			
			gpc.SetFont (font);
			PrintFooter (gpc, config);
			gpc.ShowPage ();
			gpc.EndDoc ();
		}
		
		void CreatePrintJob ()
		{
			if (printDialog != null  || printJob != null)
				return;/*
			PrintConfig config = ;
			PrintJob sourcePrintJob = new SourcePrintJob (config, Buffer);
			sourcePrintJob.upFromView = View;
			sourcePrintJob.PrintHeader = true;
			sourcePrintJob.PrintFooter = true;
			sourcePrintJob.SetHeaderFormat (GettextCatalog.GetString ("File:") +  " " +
									  StrMiddleTruncate (IdeApp.Workbench.ActiveDocument.FileName, 60), null, null, true);
			sourcePrintJob.SetFooterFormat (GettextCatalog.GetString ("MonoDevelop"), null, GettextCatalog.GetString ("Page") + " %N/%Q", true);
			sourcePrintJob.WrapMode = WrapMode.Word; */
			printJob = new Gnome.PrintJob (Gnome.PrintConfig.Default ());
			Gnome.PrintContext ctx = printJob.Context;
			MyPrint (ctx, printJob.Config); 
			printJob.Close ();
		}
		
		static string StrMiddleTruncate (string str, int truncLen)
		{
			if (str == null) 
				return "";
			if (str.Length <= truncLen) 
				return str;
			
			string delimiter = "...";
			int leftOffset = (truncLen - delimiter.Length) / 2;
			int rightOffset = str.Length - truncLen + leftOffset + delimiter.Length;
			return str.Substring (0, leftOffset) + delimiter + str.Substring (rightOffset);
		}
		#endregion
#endif
	
		#region Toolbox
		static List<TextToolboxNode> clipboardRing = new List<TextToolboxNode> ();
		static event EventHandler ClipbardRingUpdated;
		
		static SourceEditorView ()
		{
			CopyAction.Copy += delegate (string text) {
				if (String.IsNullOrEmpty (text))
					return;
				foreach (TextToolboxNode node in clipboardRing) {
					if (node.Text == text) {
						clipboardRing.Remove (node);
						break;
					}
				}
				TextToolboxNode item = new TextToolboxNode (text);
				string[] lines = text.Split ('\n');
				for (int i = 0; i < 3 && i < lines.Length; i++) {
					if (i > 0)
						item.Description += Environment.NewLine;
					string line = lines[i];
					if (line.Length > 16)
						line = line.Substring (0, 16) + "...";
					item.Description += line;
				}
				item.Category = GettextCatalog.GetString ("Clipboard ring");
				item.Icon = IdeApp.Services.PlatformService.GetPixbufForFile ("test.txt", 16);
				item.Name = text.Length > 16 ? text.Substring (0, 16) + "..." : text;
				item.Name = item.Name.Replace ("\t", "\\t");
				item.Name = item.Name.Replace ("\n", "\\n");
				clipboardRing.Add (item);
				while (clipboardRing.Count > 12) {
					clipboardRing.RemoveAt (0);
				}
				if (ClipbardRingUpdated != null)
					ClipbardRingUpdated (null, EventArgs.Empty);
			};
		}
		
		public void UpdateClipboardRing (object sender, EventArgs e)
		{
			if (ItemsChanged != null)
				ItemsChanged (this, EventArgs.Empty);
		}
		
		public IEnumerable<BaseToolboxNode> GetDynamicItems (IToolboxConsumer consumer)
		{
			CategoryToolboxNode category = new CategoryToolboxNode (GettextCatalog.GetString ("Clipboard ring"));
			category.IsDropTarget    = false;
			category.CanIconizeItems = false;
			category.IsSorted        = false;
			foreach (TextToolboxNode item in clipboardRing) {
				category.Add (item);
			}
			
			if (clipboardRing.Count == 0) {
				TextToolboxNode item = new TextToolboxNode (null);
				item.Category = GettextCatalog.GetString ("Clipboard ring");
				item.Name = null;
				category.Add (item);
			}
			return new BaseToolboxNode [] { category };
		}
		
		public event EventHandler ItemsChanged;
		
		void IToolboxConsumer.ConsumeItem (ItemToolboxNode item)
		{
			TextToolboxNode textNode = item as TextToolboxNode;
			if (textNode == null)
				return;
			TextEditor.InsertAtCaret (textNode.Text);
			TextEditor.GrabFocus ();
		}
		
		void IToolboxConsumer.DragItem (ItemToolboxNode item, Gtk.Widget source, Gdk.DragContext ctx)
		{
			TextToolboxNode textNode = item as TextToolboxNode;
			if (textNode == null)
				return;
			TextEditor.BeginDrag (textNode.Text, source, ctx);
		}
		
		System.ComponentModel.ToolboxItemFilterAttribute[] IToolboxConsumer.ToolboxFilterAttributes {
			get {
				return new System.ComponentModel.ToolboxItemFilterAttribute[] {
					
				};
			}
		}
		public Gtk.TargetEntry[] DragTargets { 
			get {
				return (Gtk.TargetEntry[])CopyAction.TargetList;
			}
		}
				
		bool IToolboxConsumer.CustomFilterSupports (ItemToolboxNode item)
		{
			return false;
		}
		
		string IToolboxConsumer.DefaultItemDomain { 
			get {
				return "Text";
			}
		}
		#endregion
		
		#region IZoomable
		bool IZoomable.EnableZoomIn {
			get {
				return SourceEditorOptions.Options.CanZoomIn;
			}
		}
		
		bool IZoomable.EnableZoomOut {
			get {
				return SourceEditorOptions.Options.CanZoomOut;
			}
		}
		
		bool IZoomable.EnableZoomReset {
			get {
				return SourceEditorOptions.Options.CanResetZoom;
			}
		}
		
		void IZoomable.ZoomIn ()
		{
			SourceEditorOptions.Options.ZoomIn ();
		}
		
		void IZoomable.ZoomOut ()
		{
			SourceEditorOptions.Options.ZoomOut ();
		}
		
		void IZoomable.ZoomReset ()
		{
			SourceEditorOptions.Options.ZoomReset ();
		}
		#endregion
	}
} 
