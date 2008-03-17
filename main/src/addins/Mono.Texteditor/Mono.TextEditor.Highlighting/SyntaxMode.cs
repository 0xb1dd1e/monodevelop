// SyntaxMode.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Xml;


namespace Mono.TextEditor.Highlighting
{
	public class SyntaxMode : Rule
	{
		public static readonly SyntaxMode Default = new SyntaxMode ();
		string mimeType;
		List<Rule> rules = new List<Rule> ();
		
		public string MimeType {
			get {
				return mimeType;
			}
		}
		
		public ReadOnlyCollection<Rule> Rules {
			get {
				return rules.AsReadOnly ();
			}
		}
		
		public SyntaxMode()
		{
		}
		
		public Chunk[] GetChunks (Document doc, Style style, LineSegment line, int offset, int length)
		{
			return new ChunkParser (doc, style, this, line).GetChunks (offset, length);
		}
		
		class ChunkParser
		{
			Rule curRule;
			SyntaxMode mode;
			Style style;
			Stack<Span> spanStack;
			Document doc;
			LineSegment line;
			readonly ChunkStyle defaultStyle;
			List<Chunk> result = new List<Chunk> ();
			Dictionary<char, Rule.Pair<Keywords, object>> tree;
			Dictionary<char, List<Span>> spanTree;
			Rule.Pair<Keywords, object> pair     = null;
			
			Span curSpan;
			int ruleStart;
			
			public ChunkParser (Document doc, Style style, SyntaxMode mode, LineSegment line)
			{
				this.doc  = doc;
				this.style = style;
				this.mode = mode;
				this.line = line;
				defaultStyle = new Mono.TextEditor.ChunkStyle (style.Default);
			}
			
			void AddChunk (ref Chunk curChunk, int length, ChunkStyle style)
			{
				if (curChunk.Length > 0) {
					result.Add (curChunk);
					curChunk = new Chunk (curChunk.EndOffset, 0, defaultStyle);
				}
				curChunk.Style = style;
				curChunk.Length = length;
				result.Add (curChunk);
				curChunk = new Chunk (curChunk.EndOffset, 0, defaultStyle);
				curChunk.Style = GetSpanStyle ();
			}
			
			ChunkStyle GetChunkStyleColor (string topColor)
			{
				ChunkStyle result ;
				if (!String.IsNullOrEmpty (topColor)) {
					result = style.GetChunkStyle (topColor);
				} else {
					result = spanStack.Count > 0 ? style.GetChunkStyle (spanStack.Peek ().Color) : defaultStyle;
				}
				return result;
			}
			
			ChunkStyle GetSpanStyle ()
			{
				if (spanStack.Count == 0)
					return defaultStyle;
				return style.GetChunkStyle (spanStack.Peek ().Color);
			}
			
			void SetTree ()
			{
				curSpan = spanStack.Count > 0 ? spanStack.Peek () : null;
				pair    = null;
				if (curSpan != null) { 
					curRule = mode.GetRule (curSpan.Rule);
					tree     = curRule.parseTree;
				} else {
					tree     = curRule.parseTree;
				}
				wordOffset = 0;
			}
			
			void SetSpan (int offset)
			{
				curSpan = spanStack.Count > 0 ? spanStack.Peek () : null;
				foreach (SemanticRule semanticRule in curRule.SemanticRules) {
					semanticRule.Analyze (this.doc, line, result, ruleStart, offset);
				}
				if (curSpan != null) { 
					curRule  = mode.GetRule (curSpan.Rule);
					spanTree = curRule.spanStarts;
				} else {
					curRule  = mode;
					spanTree = curRule.spanStarts;
				}
				
				ruleStart = offset;
			}
			
			int wordOffset =  0; 
			
			Chunk curChunk;
			int maxEnd;
			
			public Chunk[] GetChunks (int offset, int length)
			{
				curRule = mode;
				this.ruleStart = offset;
				spanStack = line.StartSpan != null ? new Stack<Span> (line.StartSpan) : new Stack<Span> ();
				SyntaxModeService.ScanSpans (doc, curRule, spanStack, line.Offset, offset);
				pair = null;
				curChunk = new Chunk (offset, 0, GetSpanStyle ());
				maxEnd = System.Math.Min (offset + length, System.Math.Min (line.Offset + line.EditableLength, doc.Length));
				
				int endOffset = 0;
				SetSpan (offset);
				SetTree ();
				bool isNoKeyword = false;
				int len = maxEnd - offset;
				string str = len > 0 ? doc.GetTextAt (offset, len) : null;
				
				for (int i = offset; i < maxEnd; i++) {
					int textOffset = i - offset;
					char ch = str [textOffset];
					
					if (curSpan != null && !String.IsNullOrEmpty (curSpan.End)) {
						if (curSpan.Escape == ch && i + 1 < maxEnd && endOffset == 0 && doc.GetCharAt (i + 1) == curSpan.End[0]) {
							curChunk.Length += 2;
							i++;
							continue;
						} else if (curSpan.End[endOffset] == ch) {
							endOffset++;
							if (endOffset >= curSpan.End.Length) {
								curChunk.Length -= curSpan.End.Length - 1;
								AddChunk (ref curChunk, curSpan.End.Length, !String.IsNullOrEmpty (curSpan.TagColor) ? style.GetChunkStyle (curSpan.TagColor) : GetSpanStyle ());
								spanStack.Pop ();
								SetSpan (i);
								SetTree ();
								curChunk.Style  = GetSpanStyle ();
								endOffset = 0;
								continue;
							}
						} else if (endOffset != 0) {
							endOffset = 0;
							if (curSpan.End[endOffset] == ch) {
								i--;
								continue;
							}
						}
						if (String.IsNullOrEmpty (curSpan.Rule)) {
							curChunk.Length++;
							continue;
						}
							
					}
					if (spanTree != null && spanTree.ContainsKey (ch)) {
						bool found = false;
						foreach (Span span in spanTree[ch]) {
							bool mismatch = false;
							for (int j = 1; j < span.Begin.Length; j++) {
								if (i + j >= doc.Length || span.Begin [j] != doc.GetCharAt (i + j)) {
									mismatch = true;
									break;
								}
							}
							
							if (!mismatch) {
								spanStack.Push (span);
//								curChunk.Length -= span.Begin.Length - 1;
								
								AddChunk (ref curChunk, span.Begin.Length, !String.IsNullOrEmpty (span.TagColor) ? style.GetChunkStyle (span.TagColor) : GetSpanStyle ());
								SetSpan (i);
								SetTree ();
								if (!String.IsNullOrEmpty (span.NextColor))
									curChunk.Style = style.GetChunkStyle (span.NextColor);
								i += span.Begin.Length - 1;
								found = true; 
								break;
							}
						}
						if (found) 
							continue;
					}
				 skip:
						;
					if (!Char.IsLetterOrDigit (ch) && ch != '_' && pair != null && pair.o1 != null) {
						curChunk.Length -= wordOffset;
						
						AddChunk (ref curChunk, wordOffset, GetChunkStyleColor (pair.o1.Color));
						isNoKeyword = false;
					}
					
					// HACK: Add '&& (Char.IsDigit (ch) || ch == '.')' for extra speedup
					if (!isNoKeyword && wordOffset == 0) {
						Match foundMatch = null;
						int   foundMatchLength = -1;
						string matchStr = str.Substring (textOffset);
						foreach (Match ruleMatch in curRule.Matches) {
							System.Text.RegularExpressions.Match match = ruleMatch.Regex.Match (matchStr);
							if (match.Success) {
								if (foundMatch == null || foundMatchLength < match.Length) {
									foundMatch = ruleMatch;
									foundMatchLength = match.Length;
								}
							}
						}
						if (foundMatch != null) {
							AddChunk (ref curChunk, foundMatchLength, GetChunkStyleColor (foundMatch.Color));
							i += foundMatchLength - 1;
							continue;
						}
					}
					
					if (tree != null) {
						if (!isNoKeyword && tree.ContainsKey (ch)) {
							wordOffset++;
							pair = tree[ch];
							tree = (Dictionary<char, Rule.Pair<Keywords, object>>)pair.o2;
						} else {
							SetTree ();
							isNoKeyword = Char.IsLetterOrDigit (ch) || ch == '_';
						}
					} else {
						SetTree ();
						isNoKeyword = false;
					}
					
					curChunk.Length++;
				}
				curChunk.Length = maxEnd - curChunk.Offset;
				if (pair != null && pair.o1 != null) {
					curChunk.Length -= wordOffset;
					AddChunk (ref curChunk, wordOffset, GetChunkStyleColor (pair.o1.Color));
					curChunk.Style = GetChunkStyleColor (pair.o1.Color);
				}
				
				if (curChunk.Length > 0) 
					result.Add (curChunk);
				
				SetSpan (maxEnd);
				return result.ToArray ();
			}
		}
		
		public Rule GetRule (string name)
		{
			foreach (Rule rule in rules) {
				if (rule.Name == name)
					return rule;
			}
			return this;
		}
		
		void AddSemanticRule (Rule rule, SemanticRule semanticRule)
		{
			if (rule != null)
				rule.SemanticRules.Add (semanticRule);
		}
		
		public void AddSemanticRule (SemanticRule semanticRule)
		{
			AddSemanticRule (this, semanticRule);
		}
		
		public void AddSemanticRule (string addToRuleName, SemanticRule semanticRule)
		{
			AddSemanticRule (GetRule (addToRuleName), semanticRule);
		}
		
		void RemoveSemanticRule (Rule rule, Type type)
		{
			if (rule != null) {
				for (int i = 0; i < rule.SemanticRules.Count; i++) {
					if (rule.SemanticRules[i].GetType () == type) {
						rule.SemanticRules.RemoveAt (i);
						i--;
					}
				}
			}
		}
		public void RemoveSemanticRule (Type type)
		{
			RemoveSemanticRule (this, type);
		}
		public void RemoveSemanticRule (string removeFromRuleName, Type type)
		{
			RemoveSemanticRule (GetRule (removeFromRuleName), type);
		}
		
		public override string ToString ()
		{
			return String.Format ("[SyntaxMode: Name={0}, MimeType={1}]", Name, MimeType);
		}
				
		new const string Node = "SyntaxMode"; 
		
		public const string MimeTypesAttribute = "mimeTypes";
		
		new public static SyntaxMode Read (XmlReader reader)
		{
			SyntaxMode result = new SyntaxMode ();
			XmlReadHelper.ReadList (reader, Node, delegate () {
				switch (reader.LocalName) {
				case Node:
					string extends = reader.GetAttribute ("extends");
					if (!String.IsNullOrEmpty (extends)) {
						result = SyntaxModeService.GetSyntaxMode (extends);
					}
					result.name     = reader.GetAttribute ("name");
					result.mimeType = reader.GetAttribute (MimeTypesAttribute);
					return true;
				case Rule.Node:
					result.rules.Add (Rule.Read (reader));
					return true;
				}
				return result.ReadNode (reader);
			});
			result.SetupSpanTree ();
			result.SetupParseTree ();
			return result;
		}
	}
}
