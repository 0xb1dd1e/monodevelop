// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace MonoDevelop.Gettext {
    
    
    public partial class TranslationProjectOptionsDialog {
        
        private Gtk.Notebook notebook1;
        
        private Gtk.VBox vbox2;
        
        private Gtk.HBox hbox4;
        
        private Gtk.Label label3;
        
        private Gtk.Entry entryPackageName;
        
        private Gtk.Frame frame1;
        
        private Gtk.Alignment GtkAlignment2;
        
        private Gtk.Table table1;
        
        private Gtk.Entry entryRelPath;
        
        private MonoDevelop.Components.FolderEntry folderentrySystemPath;
        
        private Gtk.RadioButton radiobuttonRelPath;
        
        private Gtk.RadioButton radiobuttonSystemPath;
        
        private Gtk.Label GtkLabel2;
        
        private Gtk.Frame frame2;
        
        private Gtk.Alignment GtkAlignment3;
        
        private Gtk.VBox vbox4;
        
        private Gtk.Label label4;
        
        private Gtk.Frame frame3;
        
        private Gtk.Alignment GtkAlignment4;
        
        private Gtk.Label labelInitString;
        
        private Gtk.Label GtkLabel6;
        
        private Gtk.Label label1;
        
        private Gtk.ScrolledWindow scrolledwindow1;
        
        private Gtk.TreeView treeviewProjectList;
        
        private Gtk.Label label2;
        
        private Gtk.HSeparator hseparator1;
        
        private Gtk.Button buttonCancel;
        
        private Gtk.Button buttonOk;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget MonoDevelop.Gettext.TranslationProjectOptionsDialog
            this.Name = "MonoDevelop.Gettext.TranslationProjectOptionsDialog";
            this.Title = Mono.Unix.Catalog.GetString("Translation Options");
            this.WindowPosition = ((Gtk.WindowPosition)(4));
            this.BorderWidth = ((uint)(6));
            this.HasSeparator = false;
            // Internal child MonoDevelop.Gettext.TranslationProjectOptionsDialog.VBox
            Gtk.VBox w1 = this.VBox;
            w1.Name = "dialog1_VBox";
            w1.BorderWidth = ((uint)(2));
            // Container child dialog1_VBox.Gtk.Box+BoxChild
            this.notebook1 = new Gtk.Notebook();
            this.notebook1.CanFocus = true;
            this.notebook1.Name = "notebook1";
            this.notebook1.CurrentPage = 1;
            this.notebook1.BorderWidth = ((uint)(6));
            // Container child notebook1.Gtk.Notebook+NotebookChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            this.vbox2.BorderWidth = ((uint)(6));
            // Container child vbox2.Gtk.Box+BoxChild
            this.hbox4 = new Gtk.HBox();
            this.hbox4.Name = "hbox4";
            this.hbox4.Spacing = 6;
            // Container child hbox4.Gtk.Box+BoxChild
            this.label3 = new Gtk.Label();
            this.label3.Name = "label3";
            this.label3.LabelProp = Mono.Unix.Catalog.GetString("_Package name:");
            this.label3.UseUnderline = true;
            this.hbox4.Add(this.label3);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.hbox4[this.label3]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            // Container child hbox4.Gtk.Box+BoxChild
            this.entryPackageName = new Gtk.Entry();
            this.entryPackageName.CanFocus = true;
            this.entryPackageName.Name = "entryPackageName";
            this.entryPackageName.IsEditable = true;
            this.entryPackageName.InvisibleChar = '●';
            this.hbox4.Add(this.entryPackageName);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.hbox4[this.entryPackageName]));
            w3.Position = 1;
            this.vbox2.Add(this.hbox4);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.vbox2[this.hbox4]));
            w4.Position = 0;
            w4.Expand = false;
            w4.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.frame1 = new Gtk.Frame();
            this.frame1.Name = "frame1";
            this.frame1.ShadowType = ((Gtk.ShadowType)(0));
            // Container child frame1.Gtk.Container+ContainerChild
            this.GtkAlignment2 = new Gtk.Alignment(0F, 0F, 1F, 1F);
            this.GtkAlignment2.Name = "GtkAlignment2";
            this.GtkAlignment2.LeftPadding = ((uint)(12));
            // Container child GtkAlignment2.Gtk.Container+ContainerChild
            this.table1 = new Gtk.Table(((uint)(2)), ((uint)(2)), false);
            this.table1.Name = "table1";
            this.table1.RowSpacing = ((uint)(6));
            this.table1.ColumnSpacing = ((uint)(6));
            // Container child table1.Gtk.Table+TableChild
            this.entryRelPath = new Gtk.Entry();
            this.entryRelPath.CanFocus = true;
            this.entryRelPath.Name = "entryRelPath";
            this.entryRelPath.IsEditable = true;
            this.entryRelPath.InvisibleChar = '●';
            this.table1.Add(this.entryRelPath);
            Gtk.Table.TableChild w5 = ((Gtk.Table.TableChild)(this.table1[this.entryRelPath]));
            w5.LeftAttach = ((uint)(1));
            w5.RightAttach = ((uint)(2));
            w5.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.folderentrySystemPath = new MonoDevelop.Components.FolderEntry();
            this.folderentrySystemPath.Name = "folderentrySystemPath";
            this.table1.Add(this.folderentrySystemPath);
            Gtk.Table.TableChild w6 = ((Gtk.Table.TableChild)(this.table1[this.folderentrySystemPath]));
            w6.TopAttach = ((uint)(1));
            w6.BottomAttach = ((uint)(2));
            w6.LeftAttach = ((uint)(1));
            w6.RightAttach = ((uint)(2));
            w6.XOptions = ((Gtk.AttachOptions)(4));
            w6.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.radiobuttonRelPath = new Gtk.RadioButton(Mono.Unix.Catalog.GetString("_Relative to output path:"));
            this.radiobuttonRelPath.CanFocus = true;
            this.radiobuttonRelPath.Name = "radiobuttonRelPath";
            this.radiobuttonRelPath.DrawIndicator = true;
            this.radiobuttonRelPath.UseUnderline = true;
            this.radiobuttonRelPath.Group = new GLib.SList(System.IntPtr.Zero);
            this.table1.Add(this.radiobuttonRelPath);
            Gtk.Table.TableChild w7 = ((Gtk.Table.TableChild)(this.table1[this.radiobuttonRelPath]));
            w7.XOptions = ((Gtk.AttachOptions)(4));
            w7.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.radiobuttonSystemPath = new Gtk.RadioButton(Mono.Unix.Catalog.GetString("_System path:"));
            this.radiobuttonSystemPath.CanFocus = true;
            this.radiobuttonSystemPath.Name = "radiobuttonSystemPath";
            this.radiobuttonSystemPath.DrawIndicator = true;
            this.radiobuttonSystemPath.UseUnderline = true;
            this.radiobuttonSystemPath.Group = this.radiobuttonRelPath.Group;
            this.table1.Add(this.radiobuttonSystemPath);
            Gtk.Table.TableChild w8 = ((Gtk.Table.TableChild)(this.table1[this.radiobuttonSystemPath]));
            w8.TopAttach = ((uint)(1));
            w8.BottomAttach = ((uint)(2));
            w8.XOptions = ((Gtk.AttachOptions)(4));
            w8.YOptions = ((Gtk.AttachOptions)(4));
            this.GtkAlignment2.Add(this.table1);
            this.frame1.Add(this.GtkAlignment2);
            this.GtkLabel2 = new Gtk.Label();
            this.GtkLabel2.Name = "GtkLabel2";
            this.GtkLabel2.LabelProp = Mono.Unix.Catalog.GetString("<b>Output</b>");
            this.GtkLabel2.UseMarkup = true;
            this.frame1.LabelWidget = this.GtkLabel2;
            this.vbox2.Add(this.frame1);
            Gtk.Box.BoxChild w11 = ((Gtk.Box.BoxChild)(this.vbox2[this.frame1]));
            w11.Position = 1;
            w11.Expand = false;
            w11.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.frame2 = new Gtk.Frame();
            this.frame2.Name = "frame2";
            this.frame2.ShadowType = ((Gtk.ShadowType)(0));
            // Container child frame2.Gtk.Container+ContainerChild
            this.GtkAlignment3 = new Gtk.Alignment(0F, 0F, 1F, 1F);
            this.GtkAlignment3.Name = "GtkAlignment3";
            this.GtkAlignment3.LeftPadding = ((uint)(12));
            // Container child GtkAlignment3.Gtk.Container+ContainerChild
            this.vbox4 = new Gtk.VBox();
            this.vbox4.Name = "vbox4";
            this.vbox4.Spacing = 6;
            // Container child vbox4.Gtk.Box+BoxChild
            this.label4 = new Gtk.Label();
            this.label4.Name = "label4";
            this.label4.Xalign = 0F;
            this.label4.LabelProp = Mono.Unix.Catalog.GetString("The catalog initialization string should look like:");
            this.vbox4.Add(this.label4);
            Gtk.Box.BoxChild w12 = ((Gtk.Box.BoxChild)(this.vbox4[this.label4]));
            w12.Position = 0;
            w12.Expand = false;
            w12.Fill = false;
            // Container child vbox4.Gtk.Box+BoxChild
            this.frame3 = new Gtk.Frame();
            this.frame3.Name = "frame3";
            this.frame3.ShadowType = ((Gtk.ShadowType)(1));
            // Container child frame3.Gtk.Container+ContainerChild
            this.GtkAlignment4 = new Gtk.Alignment(0F, 0F, 1F, 1F);
            this.GtkAlignment4.Name = "GtkAlignment4";
            this.GtkAlignment4.LeftPadding = ((uint)(12));
            // Container child GtkAlignment4.Gtk.Container+ContainerChild
            this.labelInitString = new Gtk.Label();
            this.labelInitString.Name = "labelInitString";
            this.labelInitString.Xalign = 0F;
            this.labelInitString.LabelProp = Mono.Unix.Catalog.GetString("Mono.Unix.Catalog.Init (\"i18n\", \"./locale\");");
            this.GtkAlignment4.Add(this.labelInitString);
            this.frame3.Add(this.GtkAlignment4);
            this.vbox4.Add(this.frame3);
            Gtk.Box.BoxChild w15 = ((Gtk.Box.BoxChild)(this.vbox4[this.frame3]));
            w15.Position = 1;
            w15.Expand = false;
            w15.Fill = false;
            this.GtkAlignment3.Add(this.vbox4);
            this.frame2.Add(this.GtkAlignment3);
            this.GtkLabel6 = new Gtk.Label();
            this.GtkLabel6.Name = "GtkLabel6";
            this.GtkLabel6.LabelProp = Mono.Unix.Catalog.GetString("<b>Init String</b>");
            this.GtkLabel6.UseMarkup = true;
            this.frame2.LabelWidget = this.GtkLabel6;
            this.vbox2.Add(this.frame2);
            Gtk.Box.BoxChild w18 = ((Gtk.Box.BoxChild)(this.vbox2[this.frame2]));
            w18.Position = 2;
            w18.Expand = false;
            w18.Fill = false;
            this.notebook1.Add(this.vbox2);
            // Notebook tab
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("Settings");
            this.notebook1.SetTabLabel(this.vbox2, this.label1);
            this.label1.ShowAll();
            // Container child notebook1.Gtk.Notebook+NotebookChild
            this.scrolledwindow1 = new Gtk.ScrolledWindow();
            this.scrolledwindow1.CanFocus = true;
            this.scrolledwindow1.Name = "scrolledwindow1";
            this.scrolledwindow1.ShadowType = ((Gtk.ShadowType)(1));
            this.scrolledwindow1.BorderWidth = ((uint)(6));
            // Container child scrolledwindow1.Gtk.Container+ContainerChild
            this.treeviewProjectList = new Gtk.TreeView();
            this.treeviewProjectList.CanFocus = true;
            this.treeviewProjectList.Name = "treeviewProjectList";
            this.treeviewProjectList.HeadersClickable = true;
            this.scrolledwindow1.Add(this.treeviewProjectList);
            this.notebook1.Add(this.scrolledwindow1);
            Gtk.Notebook.NotebookChild w21 = ((Gtk.Notebook.NotebookChild)(this.notebook1[this.scrolledwindow1]));
            w21.Position = 1;
            // Notebook tab
            this.label2 = new Gtk.Label();
            this.label2.Name = "label2";
            this.label2.LabelProp = Mono.Unix.Catalog.GetString("Include in Projects");
            this.notebook1.SetTabLabel(this.scrolledwindow1, this.label2);
            this.label2.ShowAll();
            w1.Add(this.notebook1);
            Gtk.Box.BoxChild w22 = ((Gtk.Box.BoxChild)(w1[this.notebook1]));
            w22.Position = 0;
            // Container child dialog1_VBox.Gtk.Box+BoxChild
            this.hseparator1 = new Gtk.HSeparator();
            this.hseparator1.Name = "hseparator1";
            w1.Add(this.hseparator1);
            Gtk.Box.BoxChild w23 = ((Gtk.Box.BoxChild)(w1[this.hseparator1]));
            w23.PackType = ((Gtk.PackType)(1));
            w23.Position = 2;
            w23.Expand = false;
            w23.Fill = false;
            // Internal child MonoDevelop.Gettext.TranslationProjectOptionsDialog.ActionArea
            Gtk.HButtonBox w24 = this.ActionArea;
            w24.Name = "dialog1_ActionArea";
            w24.Spacing = 6;
            w24.BorderWidth = ((uint)(5));
            w24.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.buttonCancel = new Gtk.Button();
            this.buttonCancel.CanDefault = true;
            this.buttonCancel.CanFocus = true;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseStock = true;
            this.buttonCancel.UseUnderline = true;
            this.buttonCancel.Label = "gtk-cancel";
            this.AddActionWidget(this.buttonCancel, -6);
            Gtk.ButtonBox.ButtonBoxChild w25 = ((Gtk.ButtonBox.ButtonBoxChild)(w24[this.buttonCancel]));
            w25.Expand = false;
            w25.Fill = false;
            // Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.buttonOk = new Gtk.Button();
            this.buttonOk.CanDefault = true;
            this.buttonOk.CanFocus = true;
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.UseStock = true;
            this.buttonOk.UseUnderline = true;
            this.buttonOk.Label = "gtk-ok";
            this.AddActionWidget(this.buttonOk, -5);
            Gtk.ButtonBox.ButtonBoxChild w26 = ((Gtk.ButtonBox.ButtonBoxChild)(w24[this.buttonOk]));
            w26.Position = 1;
            w26.Expand = false;
            w26.Fill = false;
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.DefaultWidth = 519;
            this.DefaultHeight = 426;
            this.folderentrySystemPath.Hide();
            this.Show();
        }
    }
}
