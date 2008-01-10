//  SharpDevelopAboutPanels.cs
//
//  This file was derived from a file from #Develop. 
//
//  Copyright (C) 2001-2007 Mike Krüger <mkrueger@novell.com>
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.IO;
using System.Text;
using System.Reflection;

using Gtk;
using MonoDevelop.Core;

namespace MonoDevelop.Ide.Gui.Dialogs
{
	internal class AboutMonoDevelopTabPage : VBox
	{
		public AboutMonoDevelopTabPage ()
		{
			BorderWidth = 6;
			Label versionLabel = new Label ();
			Label licenseLabel = new Label ();
			Label copyrightLabel = new Label ();

			versionLabel.Markup = String.Format ("<b>{0}</b>\n    {1}", GettextCatalog.GetString ("Version"), BuildVariables.PackageVersion);
			HBox hboxVersion = new HBox ();
			hboxVersion.PackStart (versionLabel, false, false, 5);
			
			HBox hboxLicense = new HBox ();
			licenseLabel.Markup = GettextCatalog.GetString ("<b>License</b>\n    {0}", GettextCatalog.GetString ("Released under the GNU General Public license."));
			hboxLicense.PackStart (licenseLabel, false, false, 5);

			HBox hboxCopyright = new HBox ();
			copyrightLabel.Markup = GettextCatalog.GetString ("<b>Copyright</b>\n    (c) 2000-2003 by icsharpcode.net\n    (c) 2004-2007 by MonoDevelop contributors");
			hboxCopyright.PackStart (copyrightLabel, false, false, 5);

			this.PackStart (hboxVersion, false, true, 0);
			this.PackStart (hboxLicense, false, true, 5);
			this.PackStart (hboxCopyright, false, true, 5);
			this.ShowAll ();
		}
	}
	
	internal class VersionInformationTabPage : VBox
	{
		public VersionInformationTabPage ()
		{
			BorderWidth = 6;
			TreeView listView = new TreeView ();
			listView.RulesHint = true;
			listView.AppendColumn (GettextCatalog.GetString ("Name"), new CellRendererText (), "text", 0);
			listView.AppendColumn (GettextCatalog.GetString ("Version"), new CellRendererText (), "text", 1);
			listView.AppendColumn (GettextCatalog.GetString ("Path"), new CellRendererText (), "text", 2);
			listView.Columns [0].Sizing = TreeViewColumnSizing.Fixed;
			listView.Columns [0].FixedWidth = 200;
			listView.Columns [0].Resizable = true;
			
			listView.Model = FillListView ();
			ScrolledWindow sw = new ScrolledWindow ();
			sw.ShadowType = Gtk.ShadowType.In;
			sw.Add (listView);
			this.PackStart (sw, true, true, 0);
		}
		
		ListStore FillListView()
		{
			ListStore store = new ListStore (typeof (string), typeof (string), typeof (string));
			
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {
				AssemblyName name = asm.GetName ();
				
				string loc;
				
				try {
					loc = System.IO.Path.GetFullPath (asm.Location);
				} catch {
					loc = GettextCatalog.GetString ("dynamic");
				}
				
				store.AppendValues (name.Name, name.Version.ToString (), loc);
			}
			
			store.SetSortColumnId (0, SortType.Ascending);
			return store;
		}
	}
}

