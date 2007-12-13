//  CombineBuildOptions.cs
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
using System.Collections;
using System.ComponentModel;

using MonoDevelop.Core;
using MonoDevelop.Core.Gui.Dialogs;
using MonoDevelop.Projects;

using Gtk;
using MonoDevelop.Components;

namespace MonoDevelop.Projects.Gui.Dialogs.OptionPanels
{
	internal class CombineBuildOptions : AbstractOptionPanel
	{
		CombineBuildOptionsWidget widget;
		
		public override void LoadPanelContents()
		{
			Add (widget = new  CombineBuildOptionsWidget ((Properties) CustomizationObject));
		}

		public override bool StorePanelContents()
		{
			bool success = widget.Store ();
 			return success;
		}					
	}

	partial class CombineBuildOptionsWidget : Gtk.Bin 
	{
		Combine combine;

		public  CombineBuildOptionsWidget (Properties CustomizationObject)
		{
			Build ();
			this.combine = ((Properties)CustomizationObject).Get<Combine> ("Combine");
			folderEntry.Path = combine.OutputDirectory;
		}

		public bool Store()
		{
			combine.OutputDirectory = folderEntry.Path;
			return true;
		}
	}
}
