// TextEditorOptions.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
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

using System;
using System.Diagnostics;
using Mono.TextEditor.Highlighting;

namespace Mono.TextEditor
{
	public class TextEditorOptions
	{
		const string DEFAULT_FONT = "Mono 10";
		static TextEditorOptions options = new TextEditorOptions ();
		
		public static TextEditorOptions Options {
			get {
				return options;
			}
			set {
				Debug.Assert (value != null);
				options = value;
			}
		}
		
		int indentationSize = 4;
		int  tabSize = 4;
		bool tabsToSpaces = false;
		bool showIconMargin = true;
		bool showLineNumberMargin = true;
		bool showFoldMargin = true;
		bool showInvalidLines = true;
		bool autoIndent = true;

		int  rulerColumn = 80;
		bool showRuler = false;
		
		bool showTabs   = false;
		bool showSpaces = false;
		bool showEolMarkers = false;
		bool enableSyntaxHighlighting = true;
		bool highlightMatchingBracket = true;
		bool highlightCaretLine = false;
		string fontName = DEFAULT_FONT;
		string colorStyle = "Default";
		
		public string IndentationString {
			get {
				return this.tabsToSpaces ? new string (' ', this.TabSize) : "\t";
			}
		}
		
		public virtual bool HighlightMatchingBracket {
			get {
				return highlightMatchingBracket;
			}
			set {
				if (value != HighlightMatchingBracket) {
					highlightMatchingBracket = value;
					OnChanged (EventArgs.Empty);
				}
			}
		}
		
		public virtual bool TabsToSpaces {
			get {
				return tabsToSpaces;
			}
			set {
				tabsToSpaces = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual int IndentationSize {
			get {
				return indentationSize;
			}
			set {
				indentationSize = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual int TabSize {
			get {
				return tabSize;
			}
			set {
				tabSize = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool ShowIconMargin {
			get {
				return showIconMargin;
			}
			set {
				showIconMargin = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool ShowLineNumberMargin {
			get {
				return showLineNumberMargin;
			}
			set {
				showLineNumberMargin = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool ShowFoldMargin {
			get {
				return showFoldMargin;
			}
			set {
				showFoldMargin = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool ShowInvalidLines {
			get {
				return showInvalidLines;
			}
			set {
				showInvalidLines = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool ShowTabs {
			get {
				return showTabs;
			}
			set {
				showTabs = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool ShowEolMarkers {
			get {
				return showEolMarkers;
			}
			set {
				showEolMarkers = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool HighlightCaretLine {
			get {
				return highlightCaretLine;
			}
			set {
				highlightCaretLine = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool ShowSpaces {
			get {
				return showSpaces;
			}
			set {
				showSpaces = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual int RulerColumn {
			get {
				return rulerColumn;
			}
			set {
				rulerColumn = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool ShowRuler {
			get {
				return showRuler;
			}
			set {
				showRuler = value;
				OnChanged (EventArgs.Empty);
			}
		}

		public virtual bool AutoIndent {
			get {
				return autoIndent;
			}
			set {
				autoIndent = value;
				OnChanged (EventArgs.Empty);
			}
		}
		
		public virtual string FontName {
			get {
				return fontName;
			}
			set {
				if (fontName != value) {
					fontName = !String.IsNullOrEmpty (value) ? value : DEFAULT_FONT;
					OnChanged (EventArgs.Empty);
				}
			}
		}
		public virtual bool EnableSyntaxHighlighting {
			get {
				return enableSyntaxHighlighting;
			}
			set {
				if (value != EnableSyntaxHighlighting) {
					enableSyntaxHighlighting = value;
					OnChanged (EventArgs.Empty);
				}
			}
		}
		
		public virtual Pango.FontDescription Font {
			get {
				return Pango.FontDescription.FromString (FontName);
			}
		}
		
		public virtual string ColorSheme {
			get {
				return colorStyle;
			}
			set {
				colorStyle = value;
				OnChanged (EventArgs.Empty);
			}
		}
		public virtual Style GetColorStyle (Gtk.Widget widget)
		{
			return SyntaxModeService.GetColorStyle (widget, ColorSheme);
		}
		
		protected static void OnChanged (EventArgs args)
		{
			if (Changed != null)
				Changed (null, args);
		}
		public static event EventHandler Changed;
	}
}
