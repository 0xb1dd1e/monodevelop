// 
// ComparisonWidgetContainer.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Linq;
using Mono.TextEditor;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Components.Diff;
using MonoDevelop.Ide.Gui.Dialogs;
using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Ide;

namespace MonoDevelop.VersionControl.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	internal partial class PatchWidget : Gtk.Bin
	{
		Mono.TextEditor.TextEditor diffEditor;
		ComparisonWidget widget;
		public ComparisonWidget ComparisonWidget {
			get {
				return this.widget;
			}
		}
		public PatchWidget (ComparisonView comparisonView, VersionControlDocumentInfo info)
		{
			this.Build ();
			diffEditor = new Mono.TextEditor.TextEditor ();
			diffEditor.Document.MimeType = "text/x-diff";
			diffEditor.Options.FontName = info.Document.TextEditorData.Options.FontName;
			diffEditor.Options.ColorScheme = info.Document.TextEditorData.Options.ColorScheme;
			diffEditor.Options.ShowFoldMargin = false;
			diffEditor.Options.ShowIconMargin = false;
			diffEditor.Options.ShowTabs = true;
			diffEditor.Options.ShowSpaces = true;
			diffEditor.Options.ShowInvalidLines = info.Document.TextEditorData.Options.ShowInvalidLines;
			diffEditor.Document.ReadOnly = true;
			scrolledwindow1.Child = diffEditor;
			diffEditor.ShowAll ();
			using (var writer = new StringWriter ()) {
				UnifiedDiff.WriteUnifiedDiff (comparisonView.Diff, writer, 
				                              System.IO.Path.GetFileName (info.Item.Path) + "    (repository)", 
				                              System.IO.Path.GetFileName (info.Item.Path) + "    (working copy)",
				                              3);
				diffEditor.Document.Text = writer.ToString ();
			}
			buttonSave.Clicked += delegate {
				var dlg = new OpenFileDialog (GettextCatalog.GetString ("Save as..."), FileChooserAction.Save) {
					TransientFor = IdeApp.Workbench.RootWindow
				};
				
				if (!dlg.Run ())
					return;
				File.WriteAllText (dlg.SelectedFile, diffEditor.Document.Text);
			};
		}
		
	}
	
}

