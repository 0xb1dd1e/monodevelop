//
// DesignerSupportService.cs: Service that provides facilities useful 
//    for visual designers.
//
// Authors:
//   Michael Hutchinson <m.j.hutchinson@gmail.com>
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (C) 2006 Michael Hutchinson
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
//
// This source code is licenced under The MIT License:
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;

using MonoDevelop.Ide.Gui;
using MonoDevelop.Core;
using MonoDevelop.DesignerSupport.PropertyGrid;
using Mono.Addins;

namespace MonoDevelop.DesignerSupport
{
	
	
	public class DesignerSupportService : AbstractService
	{
		PropertyPad propertyPad = null;
		ToolboxService toolboxService = null;
		CodeBehindService codeBehindService = new CodeBehindService ();
		IPropertyProvider[] providers;
		
		IPropertyPadProvider lastPadProvider;
		object lastComponent;
		ICustomPropertyPadProvider lastCustomProvider;
		
		#region PropertyPad
		
		public PropertyPad PropertyPad {
			get {
				return propertyPad;
			}
		}
		
		internal void SetPropertyPad (PropertyPad pad)
		{
			propertyPad = pad;
			
			if (propertyPad != null) {
				if (lastPadProvider != null) {
					object[] provs = GetPropertyProvidersForObject (lastComponent, lastPadProvider.GetPropertyProvider ());
					if (provs.Length > 0)
						propertyPad.PropertyGrid.SetCurrentObject (lastComponent, provs);
					else
						propertyPad.BlankPad ();
					
					propertyPad.PropertyGrid.Changed += OnPropertyGridChanged;		
				}
				else if (lastCustomProvider != null) {
					propertyPad.UseCustomWidget (lastCustomProvider.GetCustomPropertyWidget ());
				}
			}
		}
		
		void DisposePropertyPadProvider ()
		{
			if (lastPadProvider != null) {
				if (propertyPad != null && propertyPad.PropertyGrid != null)
					propertyPad.PropertyGrid.Changed -= OnPropertyGridChanged;
				lastPadProvider.OnEndEditing (lastComponent);
				lastPadProvider = null;
				lastComponent = null;
			}
		}
		
		void DisposeCustomPropertyPadProvider ()
		{
			if (lastCustomProvider != null) {
				lastCustomProvider.DisposeCustomPropertyWidget ();
				lastCustomProvider = null;
			}
		}
		
		internal void ResetPropertyPad ()
		{
			DisposePropertyPadProvider ();
			DisposeCustomPropertyPadProvider ();
			if (propertyPad != null)
				propertyPad.BlankPad ();
		}
		
		public void SetPropertyPadContent (IPropertyPadProvider provider)
		{
			if (provider != null) {
				// If there was a custom provider, reset it now
				DisposeCustomPropertyPadProvider ();
				
				object comp = provider.GetActiveComponent ();
				if (lastPadProvider != null && comp == lastComponent)
					return;

				DisposePropertyPadProvider ();
				
				lastPadProvider = provider;
				lastComponent = comp;
				
				if (propertyPad == null)
					return;
					
				object[] provs = GetPropertyProvidersForObject (comp, provider.GetPropertyProvider ());
				if (provs.Length > 0)
					propertyPad.PropertyGrid.SetCurrentObject (comp, provs);
				else
					propertyPad.BlankPad ();
				
				propertyPad.PropertyGrid.Changed += OnPropertyGridChanged;
			}
			else {
				ResetPropertyPad ();
			}
		}
		
		public void SetPropertyPadContent (ICustomPropertyPadProvider provider)
		{
			if (provider != null) {
				
				if (lastCustomProvider == provider)
					return;

				// If there was a pad provider reset it now.
				DisposePropertyPadProvider ();
				DisposeCustomPropertyPadProvider ();

				lastCustomProvider = provider;
				
				if (propertyPad != null)
					propertyPad.UseCustomWidget (provider.GetCustomPropertyWidget ());
			}
			else {
				ResetPropertyPad ();
			}
		}
		
		internal object[] GetPropertyProvidersForObject (object obj, object firstProvider)
		{
			if (providers == null)
				providers = (IPropertyProvider[]) AddinManager.GetExtensionObjects ("/MonoDevelop/DesignerSupport/PropertyProviders", typeof(IPropertyProvider), true);
			
			ArrayList list = new ArrayList ();
			if (firstProvider != null)
				list.Add (firstProvider);
			foreach (IPropertyProvider prov in providers)
				if (prov.SupportsObject (obj))
					list.Add (prov.CreateProvider (obj));
			return list.ToArray ();
		}
			
		void OnPropertyGridChanged (object s, EventArgs a)
		{
			if (lastPadProvider != null)
				lastPadProvider.OnChanged (lastComponent);
		}
		
		#endregion
		
		#region Toolbox
		
		public ToolboxService ToolboxService {
			get{
				//lazy load of toolbox contents
				if (toolboxService == null) {
					toolboxService = new ToolboxService ();
				}
				
				return toolboxService;
			}
		}
		
		#endregion
		
		#region CodeBehind
		
		public CodeBehindService CodeBehindService {
			get { return codeBehindService; }
		}
		
		#endregion
		
		#region IService implementations
		
		public override void InitializeService()
		{
			base.InitializeService ();
			codeBehindService.Initialise ();
			IdeApp.CommandService.RegisterCommandTargetVisitor (new PropertyPadVisitor ());
			AddinManager.ExtensionChanged += OnExtensionChanged;
		}
		
		void OnExtensionChanged (object s, ExtensionEventArgs args)
		{
			if (args.PathChanged ("MonoDevelop/DesignerSupport/PropertyProviders")) {
				providers = null;
				ResetPropertyPad ();
			}
		}
		
		#endregion
	}
	
	public static class DesignerSupport
	{
		static DesignerSupportService designerSupportService;
		
		public static DesignerSupportService Service {
			get {
				if (designerSupportService == null)
					designerSupportService = (DesignerSupportService) ServiceManager.GetService (typeof(DesignerSupportService));
				return designerSupportService;
			}
		}
	}
}
