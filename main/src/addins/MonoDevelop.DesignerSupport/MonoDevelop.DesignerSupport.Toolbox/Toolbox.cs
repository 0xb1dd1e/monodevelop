/* 
 * Toolbox.cs - A toolbox widget
 * 
 * Authors: 
 *  Michael Hutchinson <m.j.hutchinson@gmail.com>
 *  
 * Copyright (C) 2005 Michael Hutchinson
 *
 * This sourcecode is licenced under The MIT License:
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit
 * persons to whom the Software is furnished to do so, subject to the
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
 * NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using Gtk;
using System.Reflection;
using System.Collections;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.ComponentModel;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Components.Commands;

namespace MonoDevelop.DesignerSupport.Toolbox
{
	public class Toolbox : VBox
	{
		ToolboxService toolboxService;
		
		ItemToolboxNode selectedNode;
		Hashtable expandedCategories = new Hashtable ();
		
		ToolboxWidget toolboxWidget;
		ScrolledWindow scrolledWindow;

		Toolbar toolbar;
		ToggleToolButton filterToggleButton;
		ToggleToolButton catToggleButton;
		ToggleToolButton compactModeToggleButton;
		Entry filterEntry;
		
		public Toolbox (ToolboxService toolboxService)
		{			
			this.toolboxService = toolboxService;
			
			#region Toolbar
			toolbar = new Toolbar ();
			toolbar.ToolbarStyle = ToolbarStyle.Icons;
			toolbar.IconSize = IconSize.Menu;
			base.PackStart (toolbar, false, false, 0);
		
			filterToggleButton = new ToggleToolButton ();
			filterToggleButton.IconWidget = new Image (Stock.Find, IconSize.Menu);
			filterToggleButton.Toggled += new EventHandler (toggleFiltering);
			toolbar.Insert (filterToggleButton, 0);
			
			catToggleButton = new ToggleToolButton ();
			catToggleButton.IconWidget = new Image ("md-design-categorise", IconSize.Menu);
			catToggleButton.Toggled += new EventHandler (toggleCategorisation);
			toolbar.Insert (catToggleButton, 1);
			
			compactModeToggleButton = new ToggleToolButton ();
			compactModeToggleButton.IconWidget = new Image ("md-design-listboxtoggle", IconSize.Menu);
			compactModeToggleButton.Toggled += new EventHandler (ToggleCompactMode);
			toolbar.Insert (compactModeToggleButton, 2);
	
			SeparatorToolItem sep = new SeparatorToolItem();
			toolbar.Insert (sep, 3);
			
			ToolButton toolboxAddButton = new ToolButton (Stock.Add);
			toolbar.Insert (toolboxAddButton, 4);
			toolboxAddButton.Clicked += new EventHandler (toolboxAddButton_Clicked);
			
			filterEntry = new Entry();
			filterEntry.WidthRequest = 150;
			filterEntry.Changed += new EventHandler (filterTextChanged);
			
			#endregion
			
			toolboxWidget = new ToolboxWidget ();
			toolboxWidget.SelectedItemChanged += delegate {
				selectedNode = this.toolboxWidget.SelectedItem != null ? this.toolboxWidget.SelectedItem.Tag as ItemToolboxNode : null;
				toolboxService.SelectItem (selectedNode);
			};
			this.toolboxWidget.DragBegin += delegate(object sender, Gtk.DragBeginArgs e) {
				if (this.toolboxWidget.SelectedItem != null)
					toolboxService.DragSelectedItem (this.toolboxWidget, e.Context);
			};
			this.toolboxWidget.ActivateSelectedItem += delegate {
				toolboxService.UseSelectedItem ();
			};
			
			this.toolboxWidget.ButtonReleaseEvent += OnButtonRelease;
			
			scrolledWindow = new ScrolledWindow ();
			base.PackEnd (scrolledWindow, true, true, 0);
			
			//Initialise self
			scrolledWindow.ShadowType = ShadowType.None;
			scrolledWindow.VscrollbarPolicy = PolicyType.Automatic;
			scrolledWindow.HscrollbarPolicy = PolicyType.Never;
			scrolledWindow.WidthRequest = 150;
			scrolledWindow.Add (this.toolboxWidget);
			
			//update view when toolbox service updated
			toolboxService.ToolboxContentsChanged += delegate { Refresh (); };
			toolboxService.ToolboxConsumerChanged += delegate { Refresh (); };
			Refresh ();
			
			//set initial state
			filterToggleButton.Active = false;
			this.toolboxWidget.ShowCategories = catToggleButton.Active = true;
			compactModeToggleButton.Active = MonoDevelop.Core.PropertyService.Get ("ToolboxIsInCompactMode", false);
			this.toolboxWidget.IsListMode  = !compactModeToggleButton.Active;
			this.ShowAll ();
		}
		
		#region Toolbar event handlers
		
		void ToggleCompactMode (object sender, EventArgs e)
		{
			this.toolboxWidget.IsListMode = !compactModeToggleButton.Active;
			MonoDevelop.Core.PropertyService.Set ("ToolboxIsInCompactMode", compactModeToggleButton.Active);
		}
		
		void toggleFiltering (object sender, EventArgs e)
		{
			if (!filterToggleButton.Active && (base.Children.Length == 3)) {
				filterEntry.Text = "";
				base.Remove (filterEntry);
			} else if (base.Children.Length == 2) {
				base.PackStart (filterEntry, false, false, 4);
				filterEntry.Show ();
				filterEntry.GrabFocus ();
			} else throw new Exception ("Unexpected number of widgets");
		}
		
		void toggleCategorisation (object sender, EventArgs e)
		{
			this.toolboxWidget.ShowCategories = catToggleButton.Active;
		}
		
		void filterTextChanged (object sender, EventArgs e)
		{
			foreach (Item item in toolboxWidget.AllItems) {
				item.IsVisible = !filterToggleButton.Active || 
					              item.Text.ToUpper ().Contains (filterEntry.Text.ToUpper ());
			}
			toolboxWidget.QueueDraw ();
		}
		
		void toolboxAddButton_Clicked (object sender, EventArgs e)
		{
			toolboxService.AddUserItems ();
		}
		
		private void OnButtonRelease(object sender, Gtk.ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 3) {
				ShowPopup ();
			}
		}
		
		void ShowPopup ()
		{
			CommandEntrySet eset = IdeApp.CommandService.CreateCommandEntrySet ("/MonoDevelop/DesignerSupport/ToolboxItemContextMenu");
			IdeApp.CommandService.ShowContextMenu (eset, this);
		}

		[CommandHandler (MonoDevelop.Ide.Commands.EditCommands.Delete)]
		internal void OnDeleteItem ()
		{
			toolboxService.RemoveUserItem (selectedNode);
		}

		[CommandUpdateHandler (MonoDevelop.Ide.Commands.EditCommands.Delete)]
		internal void OnUpdateDeleteItem (CommandInfo info)
		{
			info.Enabled = selectedNode != null;
		}
		
		#endregion
		
		#region GUI population
		public void Refresh ()
		{
			Dictionary<string, Category> categories = new Dictionary<string, Category> ();
			foreach (BaseToolboxNode node in toolboxService.GetCurrentToolboxItems ()) {
				Item newItem = new Item (node.ViewIcon, node.Label, node.Label, node);
				if (node is ItemToolboxNode) {
					ItemToolboxNode itbn = ((ItemToolboxNode)node);
					if (!categories.ContainsKey (itbn.Category))
						categories[itbn.Category] = new Category (itbn.Category);
					categories[itbn.Category].Items.Add (newItem);
				}
			}
			Drag.SourceUnset (toolboxWidget);
			toolboxWidget.ClearCategories ();
			foreach (Category category in categories.Values) {
				category.IsExpanded = true;
				toolboxWidget.AddCategory (category);
			}
			toolboxWidget.QueueDraw ();
			Gtk.TargetEntry[] targetTable = toolboxService.GetCurrentDragTargetTable ();
			if (targetTable != null)
				Drag.SourceSet (toolboxWidget, Gdk.ModifierType.Button1Mask, targetTable, Gdk.DragAction.Copy | Gdk.DragAction.Move);
		}
		#endregion
		
	}
}
