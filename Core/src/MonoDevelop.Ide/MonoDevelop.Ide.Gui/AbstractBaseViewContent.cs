// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version value="$version"/>
// </file>

using System;
//using System.Windows.Forms;

using MonoDevelop.Core;

namespace MonoDevelop.Ide.Gui
{
	public abstract class AbstractBaseViewContent : IBaseViewContent
	{
		IWorkbenchWindow workbenchWindow = null;
		
		public abstract Gtk.Widget Control {
			get;
		}
		
		public virtual IWorkbenchWindow WorkbenchWindow {
			get {
				return workbenchWindow;
			}
			set {
				workbenchWindow = value;
				OnWorkbenchWindowChanged(EventArgs.Empty);
			}
		}
		
		public virtual string TabPageLabel {
			get {
				return GettextCatalog.GetString ("Abstract Content");
			}
		}
		
		public virtual void RedrawContent()
		{
		}
		
		public virtual void Dispose()
		{
			if (Control != null)
				Control.Dispose ();				
		}
		
		public virtual bool CanReuseView (string fileName)
		{
			return false;
		}
		
		protected virtual void OnWorkbenchWindowChanged(EventArgs e)
		{
			if (WorkbenchWindowChanged != null) {
				WorkbenchWindowChanged(this, e);
			}
		}
		
		public virtual object GetContent (Type type)
		{
			if (type.IsInstanceOfType (this))
				return this;
			else
				return null;
		}
		
		public event EventHandler WorkbenchWindowChanged;
	}
}
