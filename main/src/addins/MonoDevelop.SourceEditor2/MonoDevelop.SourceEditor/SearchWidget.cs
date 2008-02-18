//
// SearchWidget.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.SourceEditor
{
	public partial class SearchWidget : Gtk.Bin
	{
		const char historySeparator = '\n';
		const int  historyLimit = 20;
		
		internal const string seachHistoryProperty = "MonoDevelop.FindReplaceDialogs.FindHistory";
		ListStore searchHistory = new ListStore (typeof (string));
		SourceEditorWidget widget;
		internal static string searchPattern = "";
		
		public static bool IsCaseSensitive {
			get {
				return PropertyService.Get ("IsCaseSensitive", true);
			}
		}
		
		public static bool IsWholeWordOnly {
			get {
				return PropertyService.Get ("IsWholeWordOnly", false);
			}
		}
		
		void SetIsCaseSensitive (bool value)
		{
			PropertyService.Set ("IsCaseSensitive", value);
			widget.SetSearchOptions ();
		}
		
		void SetIsWholeWordOnly (bool value)
		{
			PropertyService.Set ("IsWholeWordOnly", value);
			widget.SetSearchOptions ();
		}
		
		public string SearchPattern {
			get {
				return this.entrySearch.Entry.Text;
			}
			set {
				this.entrySearch.Entry.Text = value;
			}
		}
		
		public SearchWidget (SourceEditorWidget widget)
		{
			this.Build();
			this.widget = widget;
			if (String.IsNullOrEmpty (widget.TextEditor.SearchPattern)) {
				widget.TextEditor.SearchPattern = SearchWidget.searchPattern;
			} else if (widget.TextEditor.SearchPattern != SearchWidget.searchPattern) {
				searchPattern = widget.TextEditor.SearchPattern;
				FireSearchPatternChanged ();
			}
			this.entrySearch.Entry.Text = widget.TextEditor.SearchPattern;
			this.entrySearch.Model = searchHistory;
			RestoreSearchHistory ();
			
			foreach (Gtk.Widget child in this.Children) {
				child.KeyPressEvent += delegate (object sender, Gtk.KeyPressEventArgs args) {
					if (args.Event.Key == Gdk.Key.Escape)
						widget.RemoveSearchWidget ();
				};
			}
			
			// if you change something here don"t forget the SearchAndReplaceWidget
			SearchWidget.SearchPatternChanged += UpdateSearchPattern;
			
			this.closeButton.Clicked += delegate {
				widget.RemoveSearchWidget ();
			};
			this.buttonReplaceMode.Clicked += delegate {
				widget.ShowReplaceWidget ();
			};
			
			this.entrySearch.Changed += delegate {
				widget.TextEditor.SearchPattern = SearchPattern;
				if (!SearchWidget.inSearchUpdate) {
					SearchWidget.searchPattern = SearchPattern;
					FireSearchPatternChanged ();
				}
			};
			this.entrySearch.Entry.Activated += delegate {
				UpdateSearchHistory (SearchPattern);
				widget.FindNext ();
			};
			this.buttonSearchForward.Clicked += delegate {
				UpdateSearchHistory (SearchPattern);
				widget.FindNext ();
			};
			this.buttonSearchBackward.Clicked += delegate {
				UpdateSearchHistory (SearchPattern);
				widget.FindPrevious ();
			};
			
			this.buttonOptions.Clicked += delegate {
				Menu menu = new Menu ();
				
				Gtk.CheckMenuItem caseSensitive = new Gtk.CheckMenuItem (GettextCatalog.GetString ("Case sensitive"));
				caseSensitive.Active = SearchWidget.IsCaseSensitive;
				caseSensitive.ShowToggle = true;
				caseSensitive.Toggled += delegate {
					SetIsCaseSensitive (caseSensitive.Active);
				};
				caseSensitive.ButtonPressEvent += delegate {
					caseSensitive.Toggle ();
				};
				menu.Append (caseSensitive);
				
				Gtk.CheckMenuItem wholeWordsOnly = new Gtk.CheckMenuItem (GettextCatalog.GetString ("Whole words only"));
				wholeWordsOnly.Active = SearchWidget.IsWholeWordOnly;
				wholeWordsOnly.ShowToggle = true;
				wholeWordsOnly.Toggled += delegate {
					SetIsWholeWordOnly (wholeWordsOnly.Active);
				};
				wholeWordsOnly.ButtonPressEvent += delegate {
					wholeWordsOnly.Toggle ();
				};
				menu.Append (wholeWordsOnly);
				IdeApp.CommandService.ShowContextMenu (menu);
				menu.ShowAll ();
			};
		}
		
		public override void Dispose ()
		{
			SearchPatternChanged -= UpdateSearchPattern;
			if (searchHistory != null) {
				searchHistory.Dispose ();
				searchHistory = null;
			}
			widget = null;
			base.Dispose ();
		}
		
		internal static List<string> GetHistory (string propertyKey)
		{
			string stringArray = PropertyService.Get<string> (propertyKey);
			if (String.IsNullOrEmpty (stringArray))
				return new List<string> ();
			return new List<string> (stringArray.Split (historySeparator));
		}
		
		internal static void StoreHistory (string propertyKey, List<string> history)
		{
			PropertyService.Set (propertyKey, String.Join (historySeparator.ToString (), history.ToArray ()));
		}
		
		internal static void UpdateHistory (string propertyKey, string item)
		{
			List<string> history = GetHistory (propertyKey);
			history.Remove (item);
			history.Insert (0, item);
			while (history.Count >= historyLimit) 
				history.RemoveAt (historyLimit - 1);
			
			StoreHistory (propertyKey, history);
		}
		
		void UpdateSearchHistory (string item)
		{
			SearchWidget.UpdateHistory (SearchWidget.seachHistoryProperty, item);
			RestoreSearchHistory ();
		}
		
		void RestoreSearchHistory ()
		{
			this.searchHistory.Clear ();
			foreach (string item in SearchWidget.GetHistory (seachHistoryProperty)) {
				this.searchHistory.AppendValues (item);
			}
		}
		
		void UpdateSearchPattern (object sender, EventArgs args)
		{
			this.entrySearch.Entry.Text = SearchWidget.searchPattern;
		}
		public void Focus ()
		{
			this.entrySearch.GrabFocus ();
		}
		
		internal static bool inSearchUpdate = false;
		internal static void FireSearchPatternChanged ()
		{
			inSearchUpdate = true;
			if (SearchPatternChanged != null)
				SearchPatternChanged (null, EventArgs.Empty);
			inSearchUpdate = false;
		}
		
		internal static event EventHandler SearchPatternChanged;
	}
}
