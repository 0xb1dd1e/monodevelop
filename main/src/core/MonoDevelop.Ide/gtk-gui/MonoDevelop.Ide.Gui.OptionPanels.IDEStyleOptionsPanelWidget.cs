
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.Ide.Gui.OptionPanels
{
	public partial class IDEStyleOptionsPanelWidget
	{
		private global::Gtk.VBox vbox13;
		private global::Gtk.Table table1;
		private global::Gtk.ComboBox comboLanguage;
		private global::Gtk.ComboBox comboTheme;
		private global::Gtk.Label label2;
		private global::Gtk.Label labelTheme;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.Ide.Gui.OptionPanels.IDEStyleOptionsPanelWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "MonoDevelop.Ide.Gui.OptionPanels.IDEStyleOptionsPanelWidget";
			// Container child MonoDevelop.Ide.Gui.OptionPanels.IDEStyleOptionsPanelWidget.Gtk.Container+ContainerChild
			this.vbox13 = new global::Gtk.VBox ();
			this.vbox13.Name = "vbox13";
			this.vbox13.Spacing = 6;
			// Container child vbox13.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(2)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.comboLanguage = global::Gtk.ComboBox.NewText ();
			this.comboLanguage.Name = "comboLanguage";
			this.table1.Add (this.comboLanguage);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1 [this.comboLanguage]));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.comboTheme = global::Gtk.ComboBox.NewText ();
			this.comboTheme.Name = "comboTheme";
			this.table1.Add (this.comboTheme);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1 [this.comboTheme]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 0F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("User Interface Language:");
			this.table1.Add (this.label2);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1 [this.label2]));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelTheme = new global::Gtk.Label ();
			this.labelTheme.Name = "labelTheme";
			this.labelTheme.Xalign = 0F;
			this.labelTheme.LabelProp = global::Mono.Unix.Catalog.GetString ("User Interface Theme:");
			this.table1.Add (this.labelTheme);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelTheme]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox13.Add (this.table1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox13 [this.table1]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			this.Add (this.vbox13);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Show ();
		}
	}
}
