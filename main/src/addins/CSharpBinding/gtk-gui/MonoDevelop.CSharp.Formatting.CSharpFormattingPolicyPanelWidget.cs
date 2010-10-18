
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.CSharp.Formatting
{
	public partial class CSharpFormattingPolicyPanelWidget
	{
		private global::Gtk.VBox vbox1;
		private global::Gtk.Label labelActive;
		private global::Gtk.HBox hbox1;
		private global::Gtk.ComboBox comboboxProfiles;
		private global::Gtk.Button buttonEdit;
		private global::Gtk.Button buttonRemove;
		private global::Gtk.HBox hbox2;
		private global::Gtk.Button buttonNew;
		private global::Gtk.Button buttonImport;
		private global::Gtk.Button buttonExport;
		private global::Gtk.Label label2;
		private global::Gtk.ScrolledWindow scrolledwindow1;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.CSharp.Formatting.CSharpFormattingPolicyPanelWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "MonoDevelop.CSharp.Formatting.CSharpFormattingPolicyPanelWidget";
			// Container child MonoDevelop.CSharp.Formatting.CSharpFormattingPolicyPanelWidget.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.labelActive = new global::Gtk.Label ();
			this.labelActive.Name = "labelActive";
			this.labelActive.Xalign = 0F;
			this.labelActive.LabelProp = global::Mono.Unix.Catalog.GetString ("_Active profile:");
			this.labelActive.UseUnderline = true;
			this.vbox1.Add (this.labelActive);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.labelActive]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.comboboxProfiles = global::Gtk.ComboBox.NewText ();
			this.comboboxProfiles.Name = "comboboxProfiles";
			this.hbox1.Add (this.comboboxProfiles);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.comboboxProfiles]));
			w2.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonEdit = new global::Gtk.Button ();
			this.buttonEdit.CanFocus = true;
			this.buttonEdit.Name = "buttonEdit";
			this.buttonEdit.UseStock = true;
			this.buttonEdit.UseUnderline = true;
			this.buttonEdit.Label = "gtk-edit";
			this.hbox1.Add (this.buttonEdit);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.buttonEdit]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonRemove = new global::Gtk.Button ();
			this.buttonRemove.CanFocus = true;
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.UseStock = true;
			this.buttonRemove.UseUnderline = true;
			this.buttonRemove.Label = "gtk-remove";
			this.hbox1.Add (this.buttonRemove);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.buttonRemove]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			this.vbox1.Add (this.hbox1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox1]));
			w5.Position = 1;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.buttonNew = new global::Gtk.Button ();
			this.buttonNew.CanFocus = true;
			this.buttonNew.Name = "buttonNew";
			this.buttonNew.UseStock = true;
			this.buttonNew.UseUnderline = true;
			this.buttonNew.Label = "gtk-new";
			this.hbox2.Add (this.buttonNew);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.buttonNew]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.buttonImport = new global::Gtk.Button ();
			this.buttonImport.CanFocus = true;
			this.buttonImport.Name = "buttonImport";
			this.buttonImport.UseUnderline = true;
			this.buttonImport.Label = global::Mono.Unix.Catalog.GetString ("_Import");
			this.hbox2.Add (this.buttonImport);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.buttonImport]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.buttonExport = new global::Gtk.Button ();
			this.buttonExport.CanFocus = true;
			this.buttonExport.Name = "buttonExport";
			this.buttonExport.UseUnderline = true;
			this.buttonExport.Label = global::Mono.Unix.Catalog.GetString ("E_xport");
			this.hbox2.Add (this.buttonExport);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.buttonExport]));
			w8.Position = 2;
			w8.Expand = false;
			w8.Fill = false;
			this.vbox1.Add (this.hbox2);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox2]));
			w9.Position = 2;
			w9.Expand = false;
			w9.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 0F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Preview:");
			this.vbox1.Add (this.label2);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.label2]));
			w10.Position = 3;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			this.vbox1.Add (this.scrolledwindow1);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.scrolledwindow1]));
			w11.Position = 4;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Show ();
		}
	}
}
