
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.Ide.Projects.OptionPanels
{
	internal partial class CombineInformationWidget
	{
		private global::Gtk.VBox vbox86;
		private global::Gtk.Table table14;
		private global::Gtk.Label descLabel;
		private global::Gtk.ScrolledWindow scrolledwindow14;
		private global::Gtk.TextView descView;
		private global::Gtk.Entry versEntry;
		private global::Gtk.Label versLabel;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.Ide.Projects.OptionPanels.CombineInformationWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "MonoDevelop.Ide.Projects.OptionPanels.CombineInformationWidget";
			// Container child MonoDevelop.Ide.Projects.OptionPanels.CombineInformationWidget.Gtk.Container+ContainerChild
			this.vbox86 = new global::Gtk.VBox ();
			this.vbox86.Name = "vbox86";
			// Container child vbox86.Gtk.Box+BoxChild
			this.table14 = new global::Gtk.Table (((uint)(2)), ((uint)(2)), false);
			this.table14.Name = "table14";
			this.table14.RowSpacing = ((uint)(6));
			this.table14.ColumnSpacing = ((uint)(6));
			// Container child table14.Gtk.Table+TableChild
			this.descLabel = new global::Gtk.Label ();
			this.descLabel.Name = "descLabel";
			this.descLabel.Xalign = 0F;
			this.descLabel.Yalign = 0F;
			this.descLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("_Description:");
			this.descLabel.UseUnderline = true;
			this.table14.Add (this.descLabel);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table14 [this.descLabel]));
			w1.TopAttach = ((uint)(1));
			w1.BottomAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(0));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table14.Gtk.Table+TableChild
			this.scrolledwindow14 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow14.WidthRequest = 350;
			this.scrolledwindow14.HeightRequest = 100;
			this.scrolledwindow14.Name = "scrolledwindow14";
			this.scrolledwindow14.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow14.Gtk.Container+ContainerChild
			this.descView = new global::Gtk.TextView ();
			this.descView.Name = "descView";
			this.scrolledwindow14.Add (this.descView);
			this.table14.Add (this.scrolledwindow14);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table14 [this.scrolledwindow14]));
			w3.TopAttach = ((uint)(1));
			w3.BottomAttach = ((uint)(2));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			// Container child table14.Gtk.Table+TableChild
			this.versEntry = new global::Gtk.Entry ();
			this.versEntry.Name = "versEntry";
			this.versEntry.IsEditable = true;
			this.versEntry.InvisibleChar = '●';
			this.table14.Add (this.versEntry);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table14 [this.versEntry]));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.YOptions = ((global::Gtk.AttachOptions)(0));
			// Container child table14.Gtk.Table+TableChild
			this.versLabel = new global::Gtk.Label ();
			this.versLabel.Name = "versLabel";
			this.versLabel.Xalign = 0F;
			this.versLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("_Version:");
			this.versLabel.UseUnderline = true;
			this.table14.Add (this.versLabel);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table14 [this.versLabel]));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(0));
			this.vbox86.Add (this.table14);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox86 [this.table14]));
			w6.Position = 0;
			w6.Expand = false;
			this.Add (this.vbox86);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.descLabel.MnemonicWidget = this.scrolledwindow14;
			this.versLabel.MnemonicWidget = this.versEntry;
			this.Show ();
		}
	}
}
