// Rule.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace Mono.TextEditor.Highlighting
{
	public class Rule
	{
		protected string name;
		protected string defaultColor;
		protected string delimiters = "&<>~!%^*()-+=|\\#/{}[]:;\"' ,\t.?\n\r";
		
		protected List<Keywords> keywords = new List<Keywords> ();
		protected List<Span> spans = new List<Span> ();
		protected List<Match> matches = new List<Match> ();
		protected List<Marker> prevMarker = new List<Marker> ();
		
		public List<SemanticRule> SemanticRules = new List<SemanticRule> ();
		
		public string Name {
			get {
				return name;
			}
		}
		
		public string Delimiters {
			get {
				return delimiters;
			}
		}

		public ReadOnlyCollection<Keywords> Keywords {
			get {
				return keywords.AsReadOnly ();
			}
		}
		
		public ReadOnlyCollection<Span> Spans {
			get {
				return spans.AsReadOnly ();
			}
		}
		
		public ReadOnlyCollection<Match> Matches {
			get {
				return this.matches.AsReadOnly ();
			}
		}

		public string DefaultColor {
			get {
				return defaultColor;
			}
		}

		public List<Marker> PrevMarker {
			get {
				return prevMarker;
			}
		}
		
		public Rule()
		{
		}
		
		public override string ToString ()
		{
			return String.Format ("[Rule: Name={0}, #Keywords={1}]", name, keywords.Count);
		}
		
		protected bool ReadNode (XmlReader reader)
		{
			switch (reader.LocalName) {
			case "Delimiters":
				this.delimiters = reader.ReadElementString ();
				return true;
			case Match.Node:
				this.matches.Add (Match.Read (reader));
				return true;
			case Span.Node:
			case Span.AltNode:
				this.spans.Add (Span.Read (reader));
				return true;
			case Mono.TextEditor.Highlighting.Keywords.Node:
				this.keywords.Add (Mono.TextEditor.Highlighting.Keywords.Read (reader));
				return true;
			case Marker.PrevMarker:
				this.prevMarker.Add (Marker.Read (reader));
				return true;
			}
			return false;
		}
		
		public class Pair<S, T>
		{
			public S o1;
			public T o2;
			
			public Pair (S o1, T o2)
			{
				this.o1 = o1;
				this.o2 = o2;
			}
			
			public override string ToString ()
			{
				return String.Format ("[Pair: o1={0}, o2={1}]", o1, o2);
			}
		}
		public Dictionary<char, Pair<Span, object>> spanTree = new Dictionary<char, Pair<Span, object>> ();
		public Dictionary<char, Pair<Keywords, object>> parseTree = new Dictionary<char, Pair<Keywords, object>> ();
		
		protected void SetupSpanTree ()
		{
			foreach (Span span in this.spans) {
				Dictionary<char, Pair<Span, object>> tree = spanTree;
				Pair<Span, object> pair = null;
				for (int i = 0; i < span.Begin.Length; i++) {
					if (!tree.ContainsKey (span.Begin[i]))
						tree.Add (span.Begin[i], new Pair<Span, object> (null, new Dictionary<char, Pair<Span, object>>()));
					pair = tree[span.Begin[i]];
					tree = (Dictionary<char, Pair<Span, object>>)pair.o2;
				}
				pair.o1 = span;
			}
		}
		
		void AddToÞarseTree (string key, Keywords value)
		{
			Dictionary<char, Pair<Keywords, object>> tree = parseTree;
			Pair<Keywords, object> pair = null;
			
			for (int i = 0; i < key.Length; i++) {
				if (!tree.ContainsKey (key[i]))
					tree.Add (key[i], new Pair<Keywords, object> (null, new Dictionary<char, Pair<Keywords, object>>()));
				pair = tree[key[i]];
				tree = (Dictionary<char, Pair<Keywords, object>>)pair.o2;
			}
			pair.o1 = value;
		}
		
		void AddWord (Keywords kw, string word, int i)
		{
			AddToÞarseTree (word, kw);
			char[] chars = word.ToCharArray ();
			
			if (i < word.Length) {
				chars[i] = Char.ToUpperInvariant (chars[i]);
				AddWord (kw, new string (chars), i + 1);
				
				chars[i] = Char.ToLowerInvariant (chars[i]);
				AddWord (kw, new string (chars), i + 1);
			}
		}
		
		protected void SetupParseTree ()
		{
			foreach (Keywords kw in this.keywords) {  
				foreach (string word in kw.Words)  {
					if (kw.Ignorecase) {
						AddWord (kw, word.ToLowerInvariant (), 0);
					} else {
						AddToÞarseTree (word, kw);
					}
				}
			}
		}
		
		public const string Node = "Rule";
		public static Rule Read (XmlReader reader)
		{
			Rule result = new Rule ();
			result.name         = reader.GetAttribute ("name");
			result.defaultColor = reader.GetAttribute ("color");
			XmlReadHelper.ReadList (reader, Node, delegate () {
				switch (reader.LocalName) {
				case Node:
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
