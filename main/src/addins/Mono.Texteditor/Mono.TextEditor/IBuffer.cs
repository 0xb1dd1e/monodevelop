// IBuffer.cs
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
//

using System;
using System.Text;

namespace Mono.TextEditor
{
	public interface IBuffer
	{
		int Length {
			get;
		}
		
		string Text {
			get;
			set;
		}
		
		void Insert (int offset, StringBuilder value);
		void Remove (int offset, int count);
		void Replace (int offset, int count, StringBuilder value);
		
		string GetTextAt (int offset, int count);
		string GetTextAt (ISegment segment);
		char GetCharAt (int offset);
		
		event EventHandler<ReplaceEventArgs> TextReplacing;
		event EventHandler<ReplaceEventArgs> TextReplaced;
	}
	
	public abstract class AbstractBuffer : IBuffer
	{
		public abstract int Length {
			get;
		}
		
		
		public abstract string Text {
			get;
			set;
		}
		
		public abstract void Replace (int offset, int count, StringBuilder value);
		public abstract string GetTextAt (int offset, int count);
		public abstract char GetCharAt (int offset);
		
		public void Insert (int offset, StringBuilder text)
		{
			Replace (offset, 0, text);
		}
		
		public void Remove (int offset, int count)
		{
			Replace (offset, count, null);
		}
		
		public string GetTextAt (ISegment segment)
		{
			return GetTextAt (segment.Offset, segment.Length);
		}
		
		protected virtual void OnTextReplaced (ReplaceEventArgs args)
		{
			if (TextReplaced != null)
				TextReplaced (this, args);
		}
		public event EventHandler<ReplaceEventArgs> TextReplaced;
		
		protected virtual void OnTextReplacing (ReplaceEventArgs args)
		{
			if (TextReplacing != null)
				TextReplacing (this, args);
		}
		public event EventHandler<ReplaceEventArgs> TextReplacing;
	}
}
