// ICompletionWidget.cs
//
// Author:
//   Peter Johanson  <latexer@gentoo.org>
//
// Copyright (c) 2007 Peter Johanson
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
using Gtk;

namespace MonoDevelop.Projects.Gui.Completion
{
	public interface ICompletionWidget
	{
		int TextLength { get; }
		int SelectedLength { get; }
		string GetText (int startOffset, int endOffset);
		char GetChar (int offset);
		Gtk.Style GtkStyle { get; }

		CodeCompletionContext CreateCodeCompletionContext (int triggerOffset);
		string GetCompletionText (ICodeCompletionContext ctx);
		void SetCompletionText (ICodeCompletionContext ctx, string partial_word, string complete_word);
		
		event EventHandler CompletionContextChanged;
	}
	
	public interface ICodeCompletionContext
	{
		int TriggerOffset { get; }
		int TriggerWordLength { get; }
		int TriggerLine { get; }
		int TriggerLineOffset { get; }
		int TriggerXCoord { get; }
		int TriggerYCoord { get; }
		int TriggerTextHeight { get; }
	}
	
	public class CodeCompletionContext: ICodeCompletionContext
	{
		int triggerOffset;
		int triggerWordLength;
		int triggerLine;
		int triggerLineOffset;
		int triggerXCoord;
		int triggerYCoord;
		int triggerTextHeight;
		
		public int TriggerOffset {
			get { return triggerOffset; }
			set { triggerOffset = value; }
		}
		
		public int TriggerLine {
			get { return triggerLine; }
			set { triggerLine = value; }
		}
		
		public int TriggerLineOffset {
			get { return triggerLineOffset; }
			set { triggerLineOffset = value; }
		}
		
		public int TriggerXCoord {
			get { return triggerXCoord; }
			set { triggerXCoord = value; }
		}
		
		public int TriggerYCoord {
			get { return triggerYCoord; }
			set { triggerYCoord = value; }
		}
		
		public int TriggerTextHeight {
			get { return triggerTextHeight; }
			set { triggerTextHeight = value; }
		}

		public int TriggerWordLength {
			get { return triggerWordLength; }
			set { triggerWordLength = value; }
		}
	}
}
