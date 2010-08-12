// LineSegment.cs
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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.TextEditor.Highlighting;

namespace Mono.TextEditor
{
	public class LineSegment : ISegment
	{
		internal RedBlackTree<LineSegmentTree.TreeNode>.RedBlackTreeNode treeNode;

		public RedBlackTree<LineSegmentTree.TreeNode>.RedBlackTreeIterator Iter {
			get {
				return new RedBlackTree<LineSegmentTree.TreeNode>.RedBlackTreeIterator (treeNode);
			}
		}

		static IEnumerable<TextMarker> nullMarkers = new TextMarker[0];
		List<TextMarker> markers = null;
		public IEnumerable<TextMarker> Markers {
			get {
				return markers ?? nullMarkers;
			}
		}
		public int MarkerCount {
			get {
				return markers != null ? markers.Count : 0;
			}
		}

		public int EditableLength {
			get {
				return Length - DelimiterLength;
			}
		}

		public int DelimiterLength {
			get;
			set;
		}

		public int Offset {
			get {
				return treeNode != null ? LineSegmentTree.GetOffsetFromNode (treeNode) : -1;
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public bool WasChanged {
			get;
			set;
		}

		CloneableStack<Span> startSpan = null;
		static CloneableStack<Span> emptySpan = new CloneableStack<Span> ();

		public CloneableStack<Span> StartSpan {
			get {
				return startSpan ?? emptySpan;
			}
			set {
				startSpan = value != null && value.Count == 0 ? null : value;
			}
		}

		public int Length {
			get;
			set;
		}

		public int EndOffset {
			get {
				return Offset + Length;
			}
		}

		public bool IsBookmarked  {
			get {
				if (markers == null)
					return false;
				return markers.Contains (BookmarkMarker.Instance);
			}
			set {
				if (value) {
					if (!IsBookmarked)
						AddMarker (BookmarkMarker.Instance);
				} else {
					if (markers != null)
						markers.Remove (BookmarkMarker.Instance);
				}
			}
		}

		public LineSegment (int length, int delimiterLength)
		{
			this.Length          = length;
			this.DelimiterLength = delimiterLength;
		}

		internal void AddMarker (TextMarker marker)
		{
			if (markers == null)
				markers = new List<TextMarker> ();
			marker.LineSegment = this;
			markers.Add (marker);
		}

		public void ClearMarker ()
		{
			if (markers != null) {
				markers.Clear ();
				markers = null;
			}
		}

		internal void RemoveMarker (TextMarker marker)
		{
			marker.LineSegment = null;
			if (markers == null)
				return;
			markers.Remove (marker);
			if (markers.Count == 0)
				markers = null;
		}

		internal TextMarker GetMarker (Type type)
		{
			if (markers == null)
				return null;
			return markers.Find (m => m.GetType () == type);
		}

		internal void RemoveMarker (Type type)
		{
			if (markers == null)
				return;
			for (int i = 0; i < markers.Count; i++) {
				if (markers[i].GetType () == type) {
					RemoveMarker (markers[i]);
					if (markers == null)
						return;
					i--;
				}
			}
		}

		/// <summary>
		/// This method gets the line indentation.
		/// </summary>
		/// <param name="doc">
		/// The <see cref="Document"/> the line belongs to.
		/// </param>
		/// <returns>
		/// The indentation of the line (all whitespace chars up to the first non ws char).
		/// </returns>
		public string GetIndentation (Document doc)
		{
			StringBuilder result = new StringBuilder ();
			int offset = Offset;
			int max = System.Math.Min (offset + Length, doc.Length);
			for (int i = offset; i < max; i++) {
				char ch = doc.GetCharAt (i);
				if (ch != ' ' && ch != '\t')
					break;
				result.Append (ch);
			}
			return result.ToString ();
		}

		public int GetLogicalColumn (TextEditorData editor, int visualColumn)
		{
			int curVisualColumn = 0;
			int offset = Offset;
			int max = offset + EditableLength;
			for (int i = offset; i < max; i++) {
				if (i < editor.Document.Length && editor.Document.GetCharAt (i) == '\t') {
					curVisualColumn = TextViewMargin.GetNextTabstop (editor, curVisualColumn);
				} else {
					curVisualColumn++;
				}
				if (curVisualColumn > visualColumn)
					return i - offset;
			}
			return EditableLength + (visualColumn - curVisualColumn);
		}

		public int GetVisualColumn (TextEditorData editor, int logicalColumn)
		{
			int result = 0;
			int offset = Offset;
			for (int i = 0; i < logicalColumn; i++) {
				if (i < EditableLength && editor.Document.GetCharAt (offset + i) == '\t') {
					result = TextViewMargin.GetNextTabstop (editor, result);
				} else {
					result++;
				}
			}
			return result;
		}

		public bool Contains (int offset)
		{
			int o = Offset;
			return o <= offset && offset < o + Length;
		}

		public bool Contains (ISegment segment)
		{
			return segment != null && Offset <= segment.Offset && segment.EndOffset <= EndOffset;
		}

		public override string ToString ()
		{
			return String.Format ("[LineSegment: Offset={0}, Length={1}, DelimiterLength={2}, StartSpan={3}]", this.Offset, this.Length, this.DelimiterLength, StartSpan == null ? "null" : StartSpan.Count.ToString());
		}
	}
}
