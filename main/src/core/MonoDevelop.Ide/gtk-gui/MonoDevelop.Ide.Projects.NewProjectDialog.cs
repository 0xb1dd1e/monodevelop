
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.Ide.Projects
{
	public partial class NewProjectDialog
	{
		private global::Gtk.Notebook notebook;
		private global::Gtk.VBox vbox1;
		private global::Gtk.VBox vbox2;
		private global::Gtk.HBox hbox1;
		private global::Gtk.VBox vbox4;
		private global::Gtk.HPaned hpaned1;
		private global::Gtk.ScrolledWindow scrolledwindow1;
		private global::Gtk.TreeView lst_template_types;
		private global::Gtk.HPaned panedTemplates;
		private global::Gtk.VBox boxTemplates;
		private global::Gtk.ScrolledWindow scrolledInfo;
		private global::Gtk.VBox boxInfo;
		private global::Gtk.Label labelTemplateTitle;
		private global::Gtk.Label lbl_template_descr;
		private global::Gtk.VBox vbox3;
		private global::Gtk.Table table1;
		private global::MonoDevelop.Components.FolderEntry entry_location;
		private global::Gtk.HBox hbox2;
		private global::Gtk.Entry txt_subdirectory;
		private global::Gtk.CheckButton chk_combine_directory;
		private global::Gtk.Label lbl_location;
		private global::Gtk.Label lbl_name;
		private global::Gtk.Label lbl_subdirectory;
		private global::Gtk.Entry txt_name;
		private global::Gtk.Label lbl_will_save_in;
		private global::Gtk.Label label1;
		private global::Gtk.VBox vbox5;
		private global::Gtk.Label label3;
		private global::Gtk.Label label4;
		private global::Gtk.Label label2;
		private global::Gtk.Button btn_close;
		private global::Gtk.Button btn_new;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.Ide.Projects.NewProjectDialog
			this.WidthRequest = 630;
			this.Name = "MonoDevelop.Ide.Projects.NewProjectDialog";
			this.Title = "New Solution";
			this.TypeHint = ((global::Gdk.WindowTypeHint)(1));
			this.WindowPosition = ((global::Gtk.WindowPosition)(1));
			this.BorderWidth = ((uint)(6));
			this.DestroyWithParent = true;
			// Internal child MonoDevelop.Ide.Projects.NewProjectDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog-vbox1";
			w1.Spacing = 6;
			w1.BorderWidth = ((uint)(2));
			// Container child dialog-vbox1.Gtk.Box+BoxChild
			this.notebook = new global::Gtk.Notebook ();
			this.notebook.CanFocus = true;
			this.notebook.Name = "notebook";
			this.notebook.CurrentPage = 0;
			this.notebook.ShowBorder = false;
			this.notebook.BorderWidth = ((uint)(6));
			// Container child notebook.Gtk.Notebook+NotebookChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 7;
			// Container child vbox1.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox4 = new global::Gtk.VBox ();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			// Container child vbox4.Gtk.Box+BoxChild
			this.hpaned1 = new global::Gtk.HPaned ();
			this.hpaned1.CanFocus = true;
			this.hpaned1.Name = "hpaned1";
			this.hpaned1.Position = 188;
			// Container child hpaned1.Gtk.Paned+PanedChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			this.lst_template_types = new global::Gtk.TreeView ();
			this.lst_template_types.Name = "lst_template_types";
			this.lst_template_types.HeadersVisible = false;
			this.scrolledwindow1.Add (this.lst_template_types);
			this.hpaned1.Add (this.scrolledwindow1);
			global::Gtk.Paned.PanedChild w3 = ((global::Gtk.Paned.PanedChild)(this.hpaned1 [this.scrolledwindow1]));
			w3.Resize = false;
			// Container child hpaned1.Gtk.Paned+PanedChild
			this.panedTemplates = new global::Gtk.HPaned ();
			this.panedTemplates.CanFocus = true;
			this.panedTemplates.Name = "panedTemplates";
			this.panedTemplates.Position = 308;
			// Container child panedTemplates.Gtk.Paned+PanedChild
			this.boxTemplates = new global::Gtk.VBox ();
			this.boxTemplates.Name = "boxTemplates";
			this.boxTemplates.Spacing = 6;
			this.panedTemplates.Add (this.boxTemplates);
			global::Gtk.Paned.PanedChild w4 = ((global::Gtk.Paned.PanedChild)(this.panedTemplates [this.boxTemplates]));
			w4.Resize = false;
			// Container child panedTemplates.Gtk.Paned+PanedChild
			this.scrolledInfo = new global::Gtk.ScrolledWindow ();
			this.scrolledInfo.CanFocus = true;
			this.scrolledInfo.Name = "scrolledInfo";
			this.scrolledInfo.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.scrolledInfo.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledInfo.Gtk.Container+ContainerChild
			global::Gtk.Viewport w5 = new global::Gtk.Viewport ();
			w5.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.boxInfo = new global::Gtk.VBox ();
			this.boxInfo.Name = "boxInfo";
			this.boxInfo.Spacing = 6;
			this.boxInfo.BorderWidth = ((uint)(3));
			// Container child boxInfo.Gtk.Box+BoxChild
			this.labelTemplateTitle = new global::Gtk.Label ();
			this.labelTemplateTitle.WidthRequest = 30;
			this.labelTemplateTitle.Name = "labelTemplateTitle";
			this.labelTemplateTitle.Xalign = 0F;
			this.labelTemplateTitle.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("<b>Console Project</b>");
			this.labelTemplateTitle.UseMarkup = true;
			this.labelTemplateTitle.Wrap = true;
			this.boxInfo.Add (this.labelTemplateTitle);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.boxInfo [this.labelTemplateTitle]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child boxInfo.Gtk.Box+BoxChild
			this.lbl_template_descr = new global::Gtk.Label ();
			this.lbl_template_descr.WidthRequest = 116;
			this.lbl_template_descr.Name = "lbl_template_descr";
			this.lbl_template_descr.Xalign = 0F;
			this.lbl_template_descr.Yalign = 0F;
			this.lbl_template_descr.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("Creates a new C# Project");
			this.lbl_template_descr.Wrap = true;
			this.boxInfo.Add (this.lbl_template_descr);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.boxInfo [this.lbl_template_descr]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			w5.Add (this.boxInfo);
			this.scrolledInfo.Add (w5);
			this.panedTemplates.Add (this.scrolledInfo);
			global::Gtk.Paned.PanedChild w10 = ((global::Gtk.Paned.PanedChild)(this.panedTemplates [this.scrolledInfo]));
			w10.Resize = false;
			this.hpaned1.Add (this.panedTemplates);
			this.vbox4.Add (this.hpaned1);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.hpaned1]));
			w12.Position = 0;
			this.hbox1.Add (this.vbox4);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.vbox4]));
			w13.Position = 0;
			this.vbox2.Add (this.hbox1);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox1]));
			w14.Position = 0;
			this.vbox1.Add (this.vbox2);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.vbox2]));
			w15.Position = 0;
			// Container child vbox1.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(3)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.entry_location = new global::MonoDevelop.Components.FolderEntry ();
			this.entry_location.Name = "entry_location";
			this.table1.Add (this.entry_location);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1 [this.entry_location]));
			w16.TopAttach = ((uint)(1));
			w16.BottomAttach = ((uint)(2));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.txt_subdirectory = new global::Gtk.Entry ();
			this.txt_subdirectory.Name = "txt_subdirectory";
			this.txt_subdirectory.IsEditable = true;
			this.txt_subdirectory.ActivatesDefault = true;
			this.txt_subdirectory.InvisibleChar = '●';
			this.hbox2.Add (this.txt_subdirectory);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.txt_subdirectory]));
			w17.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.chk_combine_directory = new global::Gtk.CheckButton ();
			this.chk_combine_directory.Name = "chk_combine_directory";
			this.chk_combine_directory.Label = global::MonoDevelop.Core.GettextCatalog.GetString ("_Create directory for solution");
			this.chk_combine_directory.Active = true;
			this.chk_combine_directory.DrawIndicator = true;
			this.chk_combine_directory.UseUnderline = true;
			this.hbox2.Add (this.chk_combine_directory);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.chk_combine_directory]));
			w18.Position = 1;
			w18.Expand = false;
			w18.Fill = false;
			this.table1.Add (this.hbox2);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table1 [this.hbox2]));
			w19.TopAttach = ((uint)(2));
			w19.BottomAttach = ((uint)(3));
			w19.LeftAttach = ((uint)(1));
			w19.RightAttach = ((uint)(2));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.lbl_location = new global::Gtk.Label ();
			this.lbl_location.Name = "lbl_location";
			this.lbl_location.Xalign = 0F;
			this.lbl_location.Yalign = 0F;
			this.lbl_location.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("_Location:");
			this.lbl_location.UseUnderline = true;
			this.table1.Add (this.lbl_location);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table1 [this.lbl_location]));
			w20.TopAttach = ((uint)(1));
			w20.BottomAttach = ((uint)(2));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(0));
			// Container child table1.Gtk.Table+TableChild
			this.lbl_name = new global::Gtk.Label ();
			this.lbl_name.Name = "lbl_name";
			this.lbl_name.Xalign = 0F;
			this.lbl_name.Yalign = 0F;
			this.lbl_name.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("N_ame:");
			this.lbl_name.UseUnderline = true;
			this.table1.Add (this.lbl_name);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table1 [this.lbl_name]));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(0));
			// Container child table1.Gtk.Table+TableChild
			this.lbl_subdirectory = new global::Gtk.Label ();
			this.lbl_subdirectory.Name = "lbl_subdirectory";
			this.lbl_subdirectory.Xalign = 0F;
			this.lbl_subdirectory.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("_Solution name:");
			this.lbl_subdirectory.UseUnderline = true;
			this.table1.Add (this.lbl_subdirectory);
			global::Gtk.Table.TableChild w22 = ((global::Gtk.Table.TableChild)(this.table1 [this.lbl_subdirectory]));
			w22.TopAttach = ((uint)(2));
			w22.BottomAttach = ((uint)(3));
			w22.XOptions = ((global::Gtk.AttachOptions)(4));
			w22.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.txt_name = new global::Gtk.Entry ();
			this.txt_name.Name = "txt_name";
			this.txt_name.IsEditable = true;
			this.txt_name.ActivatesDefault = true;
			this.txt_name.InvisibleChar = '●';
			this.table1.Add (this.txt_name);
			global::Gtk.Table.TableChild w23 = ((global::Gtk.Table.TableChild)(this.table1 [this.txt_name]));
			w23.LeftAttach = ((uint)(1));
			w23.RightAttach = ((uint)(2));
			w23.YOptions = ((global::Gtk.AttachOptions)(0));
			this.vbox3.Add (this.table1);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.table1]));
			w24.Position = 0;
			w24.Expand = false;
			w24.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.lbl_will_save_in = new global::Gtk.Label ();
			this.lbl_will_save_in.WidthRequest = 580;
			this.lbl_will_save_in.Name = "lbl_will_save_in";
			this.lbl_will_save_in.Xalign = 0F;
			this.lbl_will_save_in.Yalign = 0F;
			this.lbl_will_save_in.LabelProp = "Project will be saved in: /dev/null";
			this.lbl_will_save_in.Wrap = true;
			this.vbox3.Add (this.lbl_will_save_in);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.lbl_will_save_in]));
			w25.Position = 1;
			w25.Expand = false;
			w25.Fill = false;
			this.vbox1.Add (this.vbox3);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.vbox3]));
			w26.Position = 1;
			w26.Expand = false;
			this.notebook.Add (this.vbox1);
			// Notebook tab
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.LabelProp = "page1";
			this.notebook.SetTabLabel (this.vbox1, this.label1);
			this.label1.ShowAll ();
			// Container child notebook.Gtk.Notebook+NotebookChild
			this.vbox5 = new global::Gtk.VBox ();
			this.vbox5.Name = "vbox5";
			this.vbox5.Spacing = 6;
			// Container child vbox5.Gtk.Box+BoxChild
			this.label3 = new global::Gtk.Label ();
			this.label3.Name = "label3";
			this.label3.Xalign = 0F;
			this.label3.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("<b>Project Features</b>");
			this.label3.UseMarkup = true;
			this.vbox5.Add (this.label3);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.label3]));
			w28.Position = 0;
			w28.Expand = false;
			w28.Fill = false;
			// Container child vbox5.Gtk.Box+BoxChild
			this.label4 = new global::Gtk.Label ();
			this.label4.WidthRequest = 632;
			this.label4.Name = "label4";
			this.label4.Xalign = 0F;
			this.label4.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("<small>This list shows a set of features you can enable in the new project. After creating the project those features can be enabled or disabled in the Project Options dialog, or by adding new projects to the solution.</small>");
			this.label4.UseMarkup = true;
			this.label4.Wrap = true;
			this.vbox5.Add (this.label4);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.label4]));
			w29.Position = 1;
			w29.Expand = false;
			w29.Fill = false;
			this.notebook.Add (this.vbox5);
			global::Gtk.Notebook.NotebookChild w30 = ((global::Gtk.Notebook.NotebookChild)(this.notebook [this.vbox5]));
			w30.Position = 1;
			// Notebook tab
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.LabelProp = "page2";
			this.notebook.SetTabLabel (this.vbox5, this.label2);
			this.label2.ShowAll ();
			w1.Add (this.notebook);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(w1 [this.notebook]));
			w31.Position = 0;
			// Internal child MonoDevelop.Ide.Projects.NewProjectDialog.ActionArea
			global::Gtk.HButtonBox w32 = this.ActionArea;
			w32.Name = "dialog-action_area1";
			w32.Spacing = 6;
			w32.BorderWidth = ((uint)(5));
			w32.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog-action_area1.Gtk.ButtonBox+ButtonBoxChild
			this.btn_close = new global::Gtk.Button ();
			this.btn_close.CanDefault = true;
			this.btn_close.Name = "btn_close";
			this.btn_close.UseStock = true;
			this.btn_close.UseUnderline = true;
			this.btn_close.Label = "gtk-cancel";
			this.AddActionWidget (this.btn_close, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w33 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w32 [this.btn_close]));
			w33.Expand = false;
			w33.Fill = false;
			// Container child dialog-action_area1.Gtk.ButtonBox+ButtonBoxChild
			this.btn_new = new global::Gtk.Button ();
			this.btn_new.CanDefault = true;
			this.btn_new.Name = "btn_new";
			this.btn_new.UseStock = true;
			this.btn_new.UseUnderline = true;
			this.btn_new.Label = "gtk-go-forward";
			w32.Add (this.btn_new);
			global::Gtk.ButtonBox.ButtonBoxChild w34 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w32 [this.btn_new]));
			w34.Position = 1;
			w34.Expand = false;
			w34.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 735;
			this.DefaultHeight = 539;
			this.lbl_name.MnemonicWidget = this.txt_name;
			this.lbl_subdirectory.MnemonicWidget = this.txt_subdirectory;
			this.Show ();
			this.scrolledInfo.SizeAllocated += new global::Gtk.SizeAllocatedHandler (this.OnScrolledInfoSizeAllocated);
			this.boxInfo.SizeAllocated += new global::Gtk.SizeAllocatedHandler (this.OnBoxInfoSizeAllocated);
			this.txt_name.Changed += new global::System.EventHandler (this.NameChanged);
			this.txt_subdirectory.Changed += new global::System.EventHandler (this.PathChanged);
			this.chk_combine_directory.Clicked += new global::System.EventHandler (this.SolutionCheckChanged);
			this.btn_close.Clicked += new global::System.EventHandler (this.cancelClicked);
			this.btn_new.Clicked += new global::System.EventHandler (this.OpenEvent);
		}
	}
}
