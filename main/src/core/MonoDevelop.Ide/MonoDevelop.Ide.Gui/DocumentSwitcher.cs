//
// WindowSwitcher.cs
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
using Gdk;
using Gtk;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.Ide
{
	public partial class DocumentSwitcher : Gtk.Window
	{
		Gtk.ListStore padListStore;
		Gtk.ListStore documentListStore;
		Gtk.TreeView  treeviewPads, treeviewDocuments;
		
		class MyTreeView : Gtk.TreeView
		{
			protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
			{
				return false;
			}
			protected override bool OnKeyReleaseEvent (Gdk.EventKey evnt)
			{
				return false;
			}
		}
		
		public DocumentSwitcher (bool startWithNext) : base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			this.CanFocus = true;
			this.Modal = true;
			treeviewPads = new MyTreeView ();
			scrolledwindow1.Child = treeviewPads;
			
			treeviewDocuments = new MyTreeView ();
			scrolledwindow2.Child = treeviewDocuments;
			
			padListStore = new Gtk.ListStore (typeof (Gdk.Pixbuf), typeof (string), typeof (Pad));
			treeviewPads.Model = padListStore;
			treeviewPads.AppendColumn ("icon", new Gtk.CellRendererPixbuf (), "pixbuf", 0);
			treeviewPads.AppendColumn ("text", new Gtk.CellRendererText (), "text", 1);
			treeviewPads.HeadersVisible = false;
			
			treeviewPads.Selection.Changed += delegate {
				TreeIter iter;
				if (treeviewPads.Selection.GetSelected (out iter)) {
					Pad pad = padListStore.GetValue (iter, 2) as Pad;
					ShowType (IdeApp.Services.Resources.GetIcon (!String.IsNullOrEmpty (pad.Icon) ? pad.Icon : MonoDevelop.Core.Gui.Stock.MiscFiles, IconSize.Dialog),
					          pad.Title,
					          "",
					          "");
				}
			};
			documentListStore = new Gtk.ListStore (typeof (Gdk.Pixbuf), typeof (string), typeof (Document));
			treeviewDocuments.Model = documentListStore;
			treeviewDocuments.AppendColumn ("icon", new Gtk.CellRendererPixbuf (), "pixbuf", 0);
			treeviewDocuments.AppendColumn ("text", new Gtk.CellRendererText (), "text", 1);
			treeviewDocuments.HeadersVisible = false;
			treeviewDocuments.Selection.Changed += delegate {
				Document document = SelectedDocument;
				if (document != null) {
					ShowType (IdeApp.Services.Resources.GetBitmap (String.IsNullOrEmpty (document.Window.ViewContent.StockIconId) ? MonoDevelop.Core.Gui.Stock.MiscFiles : document.Window.ViewContent.StockIconId, IconSize.Dialog),
					          System.IO.Path.GetFileName (document.Title),
					          document.Window.DocumentType,
					          document.FileName);
				}
			};
			
			FillLists ();
			
			if (IdeApp.Workbench.ActiveDocument != null) {
				SelectDocument (startWithNext ? GetNextDocument (IdeApp.Workbench.ActiveDocument) : GetPrevDocument (IdeApp.Workbench.ActiveDocument));
				SwitchToDocument ();
			} else {
				SwitchToPad ();
			}
		}
		
		void SwitchToDocument ()
		{
			this.treeviewPads.Sensitive = false;
			this.treeviewDocuments.Sensitive = true;
			treeviewDocuments.GrabFocus ();
		}
		void SwitchToPad ()
		{
			this.treeviewPads.Sensitive = true;
			this.treeviewDocuments.Sensitive = false;
			treeviewPads.GrabFocus ();
		}
		
		Document GetNextDocument (Document doc)
		{
			if (IdeApp.Workbench.Documents.Count == 0)
				return null;
			int index = IdeApp.Workbench.Documents.IndexOf (doc);
			return IdeApp.Workbench.Documents [(index + 1) % IdeApp.Workbench.Documents.Count];
		}
		
		Document GetPrevDocument (Document doc)
		{
			if (IdeApp.Workbench.Documents.Count == 0)
				return null;
			int index = IdeApp.Workbench.Documents.IndexOf (doc);
			return IdeApp.Workbench.Documents [(index + IdeApp.Workbench.Documents.Count - 1) % IdeApp.Workbench.Documents.Count];
		}
		
		Document SelectedDocument {
			get {
				if (!this.treeviewDocuments.Sensitive)
					return null;
				TreeIter iter;
				if (treeviewDocuments.Selection.GetSelected (out iter)) {
					return documentListStore.GetValue (iter, 2) as Document;
				}
				return null;
			}
		}
		
		Pad GetNextPad (Pad pad)
		{
			if (this.padListStore.NColumns == 0)
				return null;
			int index = IdeApp.Workbench.Pads.IndexOf (pad);
			Pad result = IdeApp.Workbench.Pads [(index + 1) % IdeApp.Workbench.Pads.Count];
			if (!result.Visible)
				return GetNextPad (result);
			return result;
		}
				
		Pad GetPrevPad (Pad pad)
		{
			if (this.padListStore.NColumns == 0)
				return null;
			int index = IdeApp.Workbench.Pads.IndexOf (pad);
			Pad result = IdeApp.Workbench.Pads [(index + IdeApp.Workbench.Pads.Count - 1) % IdeApp.Workbench.Pads.Count];
			if (!result.Visible)
				return GetPrevPad (result);
			return result;
		}
		
		Pad SelectedPad {
			get {
				if (!this.treeviewPads.Sensitive)
					return null;
				TreeIter iter;
				if (this.treeviewPads.Selection.GetSelected (out iter)) {
					return padListStore.GetValue (iter, 2) as Pad;
				}
				return null;
			}
		}
		
		void SelectDocument (Document doc)
		{
			Gtk.TreeIter iter;
			if (documentListStore.GetIterFirst (out iter)) {
				do {
					Document curDocument = documentListStore.GetValue (iter, 2) as Document;
					if (doc == curDocument) {
						treeviewDocuments.Selection.SelectIter (iter);
						return;
					}
				} while (documentListStore.IterNext (ref iter));
			}
		}
		
		void SelectPad (Pad pad)
		{
			Gtk.TreeIter iter;
			if (padListStore.GetIterFirst (out iter)) {
				do {
					Pad curPad = padListStore.GetValue (iter, 2) as Pad;
					if (pad == curPad) {
						treeviewPads.Selection.SelectIter (iter);
						return;
					}
				} while (padListStore.IterNext (ref iter));
			}
		}
		
		void ShowType (Gdk.Pixbuf image, string title, string type, string fileName)
		{
			this.imageType.Pixbuf  = image;
			this.labelTitel.Markup =  "<span size=\"xx-large\" weight=\"bold\">" +title + "</span>";
			this.labelType.Markup =  "<span size=\"small\">" +type + "</span>";
			this.labelFileName.Text = fileName;
		}
		
		void FillLists ()
		{
			foreach (Pad pad in IdeApp.Workbench.Pads) {
				if (!pad.Visible)
					continue;
				padListStore.AppendValues (IdeApp.Services.Resources.GetBitmap (!String.IsNullOrEmpty (pad.Icon) ? pad.Icon : MonoDevelop.Core.Gui.Stock.MiscFiles, IconSize.Menu),
				                           pad.Title,
				                           pad);
			}
			
			foreach (Document doc in IdeApp.Workbench.Documents) {
				documentListStore.AppendValues (IdeApp.Services.Resources.GetBitmap (String.IsNullOrEmpty (doc.Window.ViewContent.StockIconId) ? MonoDevelop.Core.Gui.Stock.MiscFiles : doc.Window.ViewContent.StockIconId, IconSize.Menu),
				                                doc.Window.Title,
				                                doc);
			}
		}
		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			bool next = (evnt.State & Gdk.ModifierType.ShiftMask) != ModifierType.ShiftMask;
			System.Console.WriteLine (evnt.Key + " -- " + evnt.State);
			switch (evnt.Key) {
			case Gdk.Key.Left:
				SwitchToPad ();
				break;
			case Gdk.Key.Right:
				SwitchToDocument ();
				break;
			case Gdk.Key.Up:
				if (treeviewDocuments.Sensitive) {
					SelectDocument (GetPrevDocument (SelectedDocument));
				} else {
					SelectPad (GetPrevPad (SelectedPad));
				}
				break;
			case Gdk.Key.Down:
				if (treeviewDocuments.Sensitive) {
					SelectDocument (GetNextDocument (SelectedDocument));
				} else {
					SelectPad (GetNextPad (SelectedPad));
				}
				break;
			case Gdk.Key.ISO_Left_Tab:
			case Gdk.Key.Tab:
				if (treeviewDocuments.Sensitive) {
					SelectDocument (next ? GetNextDocument (SelectedDocument) : GetPrevDocument (SelectedDocument));
				} else  {
					SelectPad (next ? GetNextPad (SelectedPad) : GetPrevPad (SelectedPad));
				}
				break;
			}
			return true;
		}
		
		protected override bool OnKeyReleaseEvent (Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Control_L || evnt.Key == Gdk.Key.Control_R) {
				Document doc = SelectedDocument;
				if (doc != null) {
					doc.Select ();
				} else {
					Pad pad = SelectedPad;
					if (pad != null) {
						pad.BringToFront ();
						GLib.Timeout.Add (100, delegate {
							pad.Window.Content.Control.GrabFocus ();
							return false;
						});
					}
				}
				this.Destroy ();
			}
			return base.OnKeyReleaseEvent (evnt); 
		}
	}
}
