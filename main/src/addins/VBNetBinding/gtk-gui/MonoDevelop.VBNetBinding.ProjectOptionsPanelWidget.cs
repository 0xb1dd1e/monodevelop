// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace MonoDevelop.VBNetBinding {
    
    
    public partial class ProjectOptionsPanelWidget {
        
        private Gtk.Notebook notebook3;
        
        private Gtk.VBox vbox5;
        
        private Gtk.Label label82;
        
        private Gtk.HBox hbox2;
        
        private Gtk.Label label76;
        
        private Gtk.Table table1;
        
        private Gtk.ComboBoxEntry cmbCodePage;
        
        private Gtk.ComboBox cmbOptionCompare;
        
        private Gtk.ComboBox cmbOptionExplicit;
        
        private Gtk.ComboBox cmbOptionInfer;
        
        private Gtk.ComboBox cmbOptionStrict;
        
        private Gtk.ComboBox compileTargetCombo;
        
        private Gtk.ComboBoxEntry entryMainClass;
        
        private MonoDevelop.Components.FileEntry iconEntry;
        
        private Gtk.Label label1;
        
        private Gtk.Label label3;
        
        private Gtk.Label label4;
        
        private Gtk.Label label5;
        
        private Gtk.Label label6;
        
        private Gtk.Label label7;
        
        private Gtk.Label label8;
        
        private Gtk.Label label86;
        
        private Gtk.Label label88;
        
        private Gtk.ComboBoxEntry txtMyType;
        
        private Gtk.Label label2;
        
        private Gtk.Table table3;
        
        private Gtk.Button cmdAdd;
        
        private Gtk.ScrolledWindow GtkScrolledWindow;
        
        private Gtk.TreeView treeview1;
        
        private Gtk.Entry txtImport;
        
        private Gtk.VBox vbox1;
        
        private Gtk.Button cmdRemove;
        
        private Gtk.Label label10;
        
        private Gtk.Label label9;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget MonoDevelop.VBNetBinding.ProjectOptionsPanelWidget
            Stetic.BinContainer.Attach(this);
            this.Name = "MonoDevelop.VBNetBinding.ProjectOptionsPanelWidget";
            // Container child MonoDevelop.VBNetBinding.ProjectOptionsPanelWidget.Gtk.Container+ContainerChild
            this.notebook3 = new Gtk.Notebook();
            this.notebook3.CanFocus = true;
            this.notebook3.Name = "notebook3";
            this.notebook3.CurrentPage = 0;
            // Container child notebook3.Gtk.Notebook+NotebookChild
            this.vbox5 = new Gtk.VBox();
            this.vbox5.Name = "vbox5";
            this.vbox5.Spacing = 6;
            // Container child vbox5.Gtk.Box+BoxChild
            this.label82 = new Gtk.Label();
            this.label82.Name = "label82";
            this.label82.Xalign = 0F;
            this.label82.LabelProp = Mono.Unix.Catalog.GetString("<b>Code Generation</b>");
            this.label82.UseMarkup = true;
            this.vbox5.Add(this.label82);
            Gtk.Box.BoxChild w1 = ((Gtk.Box.BoxChild)(this.vbox5[this.label82]));
            w1.Position = 0;
            w1.Expand = false;
            w1.Fill = false;
            // Container child vbox5.Gtk.Box+BoxChild
            this.hbox2 = new Gtk.HBox();
            this.hbox2.Name = "hbox2";
            this.hbox2.Spacing = 6;
            // Container child hbox2.Gtk.Box+BoxChild
            this.label76 = new Gtk.Label();
            this.label76.WidthRequest = 18;
            this.label76.Name = "label76";
            this.hbox2.Add(this.label76);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.hbox2[this.label76]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            // Container child hbox2.Gtk.Box+BoxChild
            this.table1 = new Gtk.Table(((uint)(9)), ((uint)(2)), false);
            this.table1.Name = "table1";
            this.table1.RowSpacing = ((uint)(6));
            this.table1.ColumnSpacing = ((uint)(6));
            // Container child table1.Gtk.Table+TableChild
            this.cmbCodePage = Gtk.ComboBoxEntry.NewText();
            this.cmbCodePage.Name = "cmbCodePage";
            this.table1.Add(this.cmbCodePage);
            Gtk.Table.TableChild w3 = ((Gtk.Table.TableChild)(this.table1[this.cmbCodePage]));
            w3.TopAttach = ((uint)(3));
            w3.BottomAttach = ((uint)(4));
            w3.LeftAttach = ((uint)(1));
            w3.RightAttach = ((uint)(2));
            w3.XOptions = ((Gtk.AttachOptions)(4));
            w3.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.cmbOptionCompare = new Gtk.ComboBox();
            this.cmbOptionCompare.Name = "cmbOptionCompare";
            this.table1.Add(this.cmbOptionCompare);
            Gtk.Table.TableChild w4 = ((Gtk.Table.TableChild)(this.table1[this.cmbOptionCompare]));
            w4.TopAttach = ((uint)(7));
            w4.BottomAttach = ((uint)(8));
            w4.LeftAttach = ((uint)(1));
            w4.RightAttach = ((uint)(2));
            w4.XOptions = ((Gtk.AttachOptions)(4));
            w4.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.cmbOptionExplicit = new Gtk.ComboBox();
            this.cmbOptionExplicit.Name = "cmbOptionExplicit";
            this.table1.Add(this.cmbOptionExplicit);
            Gtk.Table.TableChild w5 = ((Gtk.Table.TableChild)(this.table1[this.cmbOptionExplicit]));
            w5.TopAttach = ((uint)(5));
            w5.BottomAttach = ((uint)(6));
            w5.LeftAttach = ((uint)(1));
            w5.RightAttach = ((uint)(2));
            w5.XOptions = ((Gtk.AttachOptions)(4));
            w5.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.cmbOptionInfer = new Gtk.ComboBox();
            this.cmbOptionInfer.Name = "cmbOptionInfer";
            this.table1.Add(this.cmbOptionInfer);
            Gtk.Table.TableChild w6 = ((Gtk.Table.TableChild)(this.table1[this.cmbOptionInfer]));
            w6.TopAttach = ((uint)(8));
            w6.BottomAttach = ((uint)(9));
            w6.LeftAttach = ((uint)(1));
            w6.RightAttach = ((uint)(2));
            w6.XOptions = ((Gtk.AttachOptions)(4));
            w6.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.cmbOptionStrict = new Gtk.ComboBox();
            this.cmbOptionStrict.Name = "cmbOptionStrict";
            this.table1.Add(this.cmbOptionStrict);
            Gtk.Table.TableChild w7 = ((Gtk.Table.TableChild)(this.table1[this.cmbOptionStrict]));
            w7.TopAttach = ((uint)(6));
            w7.BottomAttach = ((uint)(7));
            w7.LeftAttach = ((uint)(1));
            w7.RightAttach = ((uint)(2));
            w7.XOptions = ((Gtk.AttachOptions)(4));
            w7.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.compileTargetCombo = new Gtk.ComboBox();
            this.compileTargetCombo.Name = "compileTargetCombo";
            this.table1.Add(this.compileTargetCombo);
            Gtk.Table.TableChild w8 = ((Gtk.Table.TableChild)(this.table1[this.compileTargetCombo]));
            w8.LeftAttach = ((uint)(1));
            w8.RightAttach = ((uint)(2));
            w8.XOptions = ((Gtk.AttachOptions)(4));
            w8.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.entryMainClass = new Gtk.ComboBoxEntry();
            this.entryMainClass.Name = "entryMainClass";
            this.table1.Add(this.entryMainClass);
            Gtk.Table.TableChild w9 = ((Gtk.Table.TableChild)(this.table1[this.entryMainClass]));
            w9.TopAttach = ((uint)(1));
            w9.BottomAttach = ((uint)(2));
            w9.LeftAttach = ((uint)(1));
            w9.RightAttach = ((uint)(2));
            w9.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.iconEntry = new MonoDevelop.Components.FileEntry();
            this.iconEntry.Name = "iconEntry";
            this.table1.Add(this.iconEntry);
            Gtk.Table.TableChild w10 = ((Gtk.Table.TableChild)(this.table1[this.iconEntry]));
            w10.TopAttach = ((uint)(2));
            w10.BottomAttach = ((uint)(3));
            w10.LeftAttach = ((uint)(1));
            w10.RightAttach = ((uint)(2));
            w10.XOptions = ((Gtk.AttachOptions)(4));
            w10.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.Xalign = 0F;
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("Option Infer:");
            this.table1.Add(this.label1);
            Gtk.Table.TableChild w11 = ((Gtk.Table.TableChild)(this.table1[this.label1]));
            w11.TopAttach = ((uint)(8));
            w11.BottomAttach = ((uint)(9));
            w11.XOptions = ((Gtk.AttachOptions)(4));
            w11.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label3 = new Gtk.Label();
            this.label3.Name = "label3";
            this.label3.Xalign = 0F;
            this.label3.LabelProp = Mono.Unix.Catalog.GetString("Win32 Icon:");
            this.table1.Add(this.label3);
            Gtk.Table.TableChild w12 = ((Gtk.Table.TableChild)(this.table1[this.label3]));
            w12.TopAttach = ((uint)(2));
            w12.BottomAttach = ((uint)(3));
            w12.XOptions = ((Gtk.AttachOptions)(4));
            w12.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label4 = new Gtk.Label();
            this.label4.Name = "label4";
            this.label4.Xalign = 0F;
            this.label4.LabelProp = Mono.Unix.Catalog.GetString("My Type:");
            this.table1.Add(this.label4);
            Gtk.Table.TableChild w13 = ((Gtk.Table.TableChild)(this.table1[this.label4]));
            w13.TopAttach = ((uint)(4));
            w13.BottomAttach = ((uint)(5));
            w13.XOptions = ((Gtk.AttachOptions)(4));
            w13.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label5 = new Gtk.Label();
            this.label5.Name = "label5";
            this.label5.Xalign = 0F;
            this.label5.LabelProp = Mono.Unix.Catalog.GetString("Option Explicit:");
            this.table1.Add(this.label5);
            Gtk.Table.TableChild w14 = ((Gtk.Table.TableChild)(this.table1[this.label5]));
            w14.TopAttach = ((uint)(5));
            w14.BottomAttach = ((uint)(6));
            w14.XOptions = ((Gtk.AttachOptions)(4));
            w14.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label6 = new Gtk.Label();
            this.label6.Name = "label6";
            this.label6.Xalign = 0F;
            this.label6.LabelProp = Mono.Unix.Catalog.GetString("Option Strict:");
            this.table1.Add(this.label6);
            Gtk.Table.TableChild w15 = ((Gtk.Table.TableChild)(this.table1[this.label6]));
            w15.TopAttach = ((uint)(6));
            w15.BottomAttach = ((uint)(7));
            w15.XOptions = ((Gtk.AttachOptions)(4));
            w15.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label7 = new Gtk.Label();
            this.label7.Name = "label7";
            this.label7.Xalign = 0F;
            this.label7.LabelProp = Mono.Unix.Catalog.GetString("Option Compare:");
            this.table1.Add(this.label7);
            Gtk.Table.TableChild w16 = ((Gtk.Table.TableChild)(this.table1[this.label7]));
            w16.TopAttach = ((uint)(7));
            w16.BottomAttach = ((uint)(8));
            w16.XOptions = ((Gtk.AttachOptions)(4));
            w16.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label8 = new Gtk.Label();
            this.label8.Name = "label8";
            this.label8.Xalign = 0F;
            this.label8.LabelProp = Mono.Unix.Catalog.GetString("Compiler Code Page:");
            this.table1.Add(this.label8);
            Gtk.Table.TableChild w17 = ((Gtk.Table.TableChild)(this.table1[this.label8]));
            w17.TopAttach = ((uint)(3));
            w17.BottomAttach = ((uint)(4));
            w17.XOptions = ((Gtk.AttachOptions)(4));
            w17.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label86 = new Gtk.Label();
            this.label86.Name = "label86";
            this.label86.Xalign = 0F;
            this.label86.LabelProp = Mono.Unix.Catalog.GetString("Compile _Target:");
            this.label86.UseUnderline = true;
            this.table1.Add(this.label86);
            Gtk.Table.TableChild w18 = ((Gtk.Table.TableChild)(this.table1[this.label86]));
            w18.XOptions = ((Gtk.AttachOptions)(4));
            w18.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label88 = new Gtk.Label();
            this.label88.Name = "label88";
            this.label88.Xalign = 0F;
            this.label88.LabelProp = Mono.Unix.Catalog.GetString("_Main Class:");
            this.label88.UseUnderline = true;
            this.table1.Add(this.label88);
            Gtk.Table.TableChild w19 = ((Gtk.Table.TableChild)(this.table1[this.label88]));
            w19.TopAttach = ((uint)(1));
            w19.BottomAttach = ((uint)(2));
            w19.XOptions = ((Gtk.AttachOptions)(4));
            w19.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.txtMyType = new Gtk.ComboBoxEntry();
            this.txtMyType.Name = "txtMyType";
            this.table1.Add(this.txtMyType);
            Gtk.Table.TableChild w20 = ((Gtk.Table.TableChild)(this.table1[this.txtMyType]));
            w20.TopAttach = ((uint)(4));
            w20.BottomAttach = ((uint)(5));
            w20.LeftAttach = ((uint)(1));
            w20.RightAttach = ((uint)(2));
            w20.XOptions = ((Gtk.AttachOptions)(4));
            w20.YOptions = ((Gtk.AttachOptions)(4));
            this.hbox2.Add(this.table1);
            Gtk.Box.BoxChild w21 = ((Gtk.Box.BoxChild)(this.hbox2[this.table1]));
            w21.Position = 1;
            w21.Expand = false;
            w21.Fill = false;
            this.vbox5.Add(this.hbox2);
            Gtk.Box.BoxChild w22 = ((Gtk.Box.BoxChild)(this.vbox5[this.hbox2]));
            w22.Position = 1;
            w22.Expand = false;
            w22.Fill = false;
            this.notebook3.Add(this.vbox5);
            // Notebook tab
            this.label2 = new Gtk.Label();
            this.label2.Name = "label2";
            this.label2.LabelProp = Mono.Unix.Catalog.GetString("Code Generation");
            this.notebook3.SetTabLabel(this.vbox5, this.label2);
            this.label2.ShowAll();
            // Container child notebook3.Gtk.Notebook+NotebookChild
            this.table3 = new Gtk.Table(((uint)(2)), ((uint)(2)), false);
            this.table3.Name = "table3";
            this.table3.RowSpacing = ((uint)(6));
            this.table3.ColumnSpacing = ((uint)(6));
            // Container child table3.Gtk.Table+TableChild
            this.cmdAdd = new Gtk.Button();
            this.cmdAdd.CanFocus = true;
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.UseUnderline = true;
            this.cmdAdd.Label = Mono.Unix.Catalog.GetString("Add");
            this.table3.Add(this.cmdAdd);
            Gtk.Table.TableChild w24 = ((Gtk.Table.TableChild)(this.table3[this.cmdAdd]));
            w24.LeftAttach = ((uint)(1));
            w24.RightAttach = ((uint)(2));
            w24.XOptions = ((Gtk.AttachOptions)(4));
            w24.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table3.Gtk.Table+TableChild
            this.GtkScrolledWindow = new Gtk.ScrolledWindow();
            this.GtkScrolledWindow.Name = "GtkScrolledWindow";
            this.GtkScrolledWindow.ShadowType = ((Gtk.ShadowType)(1));
            // Container child GtkScrolledWindow.Gtk.Container+ContainerChild
            this.treeview1 = new Gtk.TreeView();
            this.treeview1.CanFocus = true;
            this.treeview1.Name = "treeview1";
            this.GtkScrolledWindow.Add(this.treeview1);
            this.table3.Add(this.GtkScrolledWindow);
            Gtk.Table.TableChild w26 = ((Gtk.Table.TableChild)(this.table3[this.GtkScrolledWindow]));
            w26.TopAttach = ((uint)(1));
            w26.BottomAttach = ((uint)(2));
            // Container child table3.Gtk.Table+TableChild
            this.txtImport = new Gtk.Entry();
            this.txtImport.CanFocus = true;
            this.txtImport.Name = "txtImport";
            this.txtImport.IsEditable = true;
            this.txtImport.InvisibleChar = '●';
            this.table3.Add(this.txtImport);
            Gtk.Table.TableChild w27 = ((Gtk.Table.TableChild)(this.table3[this.txtImport]));
            w27.XOptions = ((Gtk.AttachOptions)(4));
            w27.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table3.Gtk.Table+TableChild
            this.vbox1 = new Gtk.VBox();
            this.vbox1.Name = "vbox1";
            this.vbox1.Spacing = 6;
            // Container child vbox1.Gtk.Box+BoxChild
            this.cmdRemove = new Gtk.Button();
            this.cmdRemove.CanFocus = true;
            this.cmdRemove.Name = "cmdRemove";
            this.cmdRemove.UseUnderline = true;
            this.cmdRemove.Label = Mono.Unix.Catalog.GetString("Remove");
            this.vbox1.Add(this.cmdRemove);
            Gtk.Box.BoxChild w28 = ((Gtk.Box.BoxChild)(this.vbox1[this.cmdRemove]));
            w28.Position = 0;
            w28.Expand = false;
            w28.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.label10 = new Gtk.Label();
            this.label10.Name = "label10";
            this.vbox1.Add(this.label10);
            Gtk.Box.BoxChild w29 = ((Gtk.Box.BoxChild)(this.vbox1[this.label10]));
            w29.Position = 1;
            w29.Expand = false;
            w29.Fill = false;
            this.table3.Add(this.vbox1);
            Gtk.Table.TableChild w30 = ((Gtk.Table.TableChild)(this.table3[this.vbox1]));
            w30.TopAttach = ((uint)(1));
            w30.BottomAttach = ((uint)(2));
            w30.LeftAttach = ((uint)(1));
            w30.RightAttach = ((uint)(2));
            w30.XOptions = ((Gtk.AttachOptions)(4));
            w30.YOptions = ((Gtk.AttachOptions)(4));
            this.notebook3.Add(this.table3);
            Gtk.Notebook.NotebookChild w31 = ((Gtk.Notebook.NotebookChild)(this.notebook3[this.table3]));
            w31.Position = 1;
            // Notebook tab
            this.label9 = new Gtk.Label();
            this.label9.Name = "label9";
            this.label9.LabelProp = Mono.Unix.Catalog.GetString("Imports");
            this.notebook3.SetTabLabel(this.table3, this.label9);
            this.label9.ShowAll();
            this.Add(this.notebook3);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.Hide();
            this.cmdRemove.Clicked += new System.EventHandler(this.OnCmdRemoveClicked);
            this.txtImport.Changed += new System.EventHandler(this.OnTxtImportChanged);
            this.cmdAdd.Clicked += new System.EventHandler(this.OnCmdAddClicked);
        }
    }
}
