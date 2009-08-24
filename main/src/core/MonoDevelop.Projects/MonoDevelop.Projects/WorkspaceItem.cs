// 
// WorkspaceItem.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Diagnostics;
using System.CodeDom.Compiler;

using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Core.Serialization;

namespace MonoDevelop.Projects
{
	public abstract class WorkspaceItem : IBuildTarget, IWorkspaceFileObject, ILoadController
	{
		Workspace parentWorkspace;
		FileFormat format;
		Hashtable extendedProperties;
		FilePath fileName;
		int loading;
		PropertyBag userProperties;
		
		[ProjectPathItemProperty ("BaseDirectory", DefaultValue=null)]
		FilePath baseDirectory;
		
		Dictionary<string,DateTime> lastSaveTime = new Dictionary<string,DateTime> ();
		Dictionary<string,DateTime> reloadCheckTime = new Dictionary<string,DateTime> ();
		bool savingFlag;
		
		public Workspace ParentWorkspace {
			get { return parentWorkspace; }
			internal set { parentWorkspace = value; }
		}
		
		public IDictionary ExtendedProperties {
			get {
				if (extendedProperties == null)
					extendedProperties = new Hashtable ();
				return extendedProperties;
			}
		}
		
		// User properties are only loaded when the project is loaded in the IDE.
		public virtual PropertyBag UserProperties {
			get {
				if (userProperties == null)
					userProperties = new PropertyBag ();
				return userProperties; 
			}
		}
		
		// Initializes the user properties of the item
		public virtual void LoadUserProperties (PropertyBag properties)
		{
			if (userProperties != null)
				throw new InvalidOperationException ("User properties already loaded.");
			userProperties = properties;
		}
		
		public virtual string Name {
			get {
				if (fileName.IsNullOrEmpty)
					return string.Empty;
				else
					return fileName.FileNameWithoutExtension;
			}
			set {
				if (fileName.IsNullOrEmpty)
					SetLocation (FilePath.Empty, value);
				else {
					FilePath dir = fileName.ParentDirectory;
					string ext = fileName.Extension;
					FileName = dir.Combine (value) + ext;
				}
			}
		}
		
		public virtual FilePath FileName {
			get {
				return fileName;
			}
			set {
				string oldName = Name;
				fileName = value;
				if (FileFormat != null)
					fileName = FileFormat.GetValidFileName (this, fileName);
				if (oldName != Name)
					OnNameChanged (new WorkspaceItemRenamedEventArgs (this, oldName, Name));
				NotifyModified ();
			}
		}

		public void SetLocation (FilePath baseDirectory, string name)
		{
			// Add a dummy extension to the file name.
			// It will be replaced by the correct one, depending on the format
			FileName = baseDirectory.Combine (name) + ".x";
		}
		
		public FilePath BaseDirectory {
			get {
				if (baseDirectory.IsNull)
					return FileName.ParentDirectory.FullPath;
				else
					return baseDirectory;
			}
			set {
				if (!value.IsNull && !FileName.IsNull && FileName.ParentDirectory.FullPath == value.FullPath)
					baseDirectory = null;
				else if (value.IsNullOrEmpty)
					baseDirectory = null;
				else
					baseDirectory = value.FullPath;
				NotifyModified ();
			}
		}
		
		public FilePath ItemDirectory {
			get { return FileName.ParentDirectory.FullPath; }
		}
		
		protected bool Loading {
			get { return loading > 0; }
		}
		
		public WorkspaceItem ()
		{
			MonoDevelop.Projects.Extensions.ProjectExtensionUtil.LoadControl (this);
		}

		public virtual List<FilePath> GetItemFiles (bool includeReferencedFiles)
		{
			List<FilePath> col = FileFormat.Format.GetItemFiles (this);
			if (!string.IsNullOrEmpty (FileName) && !col.Contains (FileName))
				col.Add (FileName);
			return col;
		}
		
		public virtual SolutionEntityItem FindSolutionItem (string fileName)
		{
			return null;
		}
		
		public virtual bool ContainsItem (IWorkspaceObject obj)
		{
			return this == obj;
		}
		
		public ReadOnlyCollection<SolutionItem> GetAllSolutionItems ()
		{
			return GetAllSolutionItems<SolutionItem> ();
		}
		
		public virtual ReadOnlyCollection<T> GetAllSolutionItems<T> () where T: SolutionItem
		{
			return new List<T> ().AsReadOnly ();
		}
		
		public ReadOnlyCollection<Project> GetAllProjects ()
		{
			return GetAllSolutionItems<Project> ();
		}
		
		public virtual ReadOnlyCollection<Solution> GetAllSolutions ()
		{
			return GetAllItems<Solution> ();
		}
		
		public ReadOnlyCollection<WorkspaceItem> GetAllItems ()
		{
			return GetAllItems<WorkspaceItem> ();
		}
		
		public virtual ReadOnlyCollection<T> GetAllItems<T> () where T: WorkspaceItem
		{
			List<T> list = new List<T> ();
			if (this is T)
				list.Add ((T)this);
			return list.AsReadOnly ();
		}

		public virtual Project GetProjectContainingFile (FilePath fileName)
		{
			return null;
		}
		
		public virtual ReadOnlyCollection<string> GetConfigurations ()
		{
			return new ReadOnlyCollection<string> (new string [0]);
		}
		
		protected internal virtual void OnSave (IProgressMonitor monitor)
		{
			Services.ProjectService.InternalWriteWorkspaceItem (monitor, FileName, this);
		}
		
		internal void SetParentWorkspace (Workspace workspace)
		{
			parentWorkspace = workspace;
		}
		
		public BuildResult RunTarget (IProgressMonitor monitor, string target, string configuration)
		{
			return Services.ProjectService.ExtensionChain.RunTarget (monitor, this, target, configuration);
		}
		
		public void Clean (IProgressMonitor monitor, string configuration)
		{
			Services.ProjectService.ExtensionChain.RunTarget (monitor, this, ProjectService.CleanTarget, configuration);
		}
		
		public BuildResult Build (IProgressMonitor monitor, string configuration)
		{
			return InternalBuild (monitor, configuration);
		}
		
		public void Execute (IProgressMonitor monitor, ExecutionContext context, string configuration)
		{
			Services.ProjectService.ExtensionChain.Execute (monitor, this, context, configuration);
		}
		
		public bool CanExecute (ExecutionContext context, string configuration)
		{
			return Services.ProjectService.ExtensionChain.CanExecute (this, context, configuration);
		}
		
		public bool NeedsBuilding (string configuration)
		{
			return Services.ProjectService.ExtensionChain.GetNeedsBuilding (this, configuration);
		}
		
		public void SetNeedsBuilding (bool value)
		{
			foreach (string conf in GetConfigurations ())
				SetNeedsBuilding (value, conf);
		}
		
		public void SetNeedsBuilding (bool needsBuilding, string configuration)
		{
			Services.ProjectService.ExtensionChain.SetNeedsBuilding (this, needsBuilding, configuration);
		}
		
		public virtual FileFormat FileFormat {
			get {
				if (format == null) {
					format = Services.ProjectService.GetDefaultFormat (this);
				}
				return format;
			}
		}
		
		public virtual void ConvertToFormat (FileFormat format, bool convertChildren)
		{
			this.format = format;
			if (!string.IsNullOrEmpty (FileName))
				FileName = format.GetValidFileName (this, FileName);
		}
		
		internal virtual BuildResult InternalBuild (IProgressMonitor monitor, string configuration)
		{
			return Services.ProjectService.ExtensionChain.RunTarget (monitor, this, ProjectService.BuildTarget, configuration);
		}
		
		protected virtual void OnConfigurationsChanged ()
		{
			if (ConfigurationsChanged != null)
				ConfigurationsChanged (this, EventArgs.Empty);
			if (ParentWorkspace != null)
				ParentWorkspace.OnConfigurationsChanged ();
		}

		public void Save (FilePath fileName, IProgressMonitor monitor)
		{
			FileName = fileName;
			Save (monitor);
		}
		
		public void Save (IProgressMonitor monitor)
		{
			try {
				savingFlag = true;
				Services.ProjectService.ExtensionChain.Save (monitor, this);
				OnSaved (new WorkspaceItemEventArgs (this));
				
				// Update save times
				lastSaveTime.Clear ();
				reloadCheckTime.Clear ();
				foreach (FilePath file in GetItemFiles (false))
					lastSaveTime [file] = reloadCheckTime [file] = GetLastWriteTime (file);
				
				FileService.NotifyFileChanged (FileName);
			} finally {
				savingFlag = false;
			}
		}
		
		public virtual bool NeedsReload {
			get {
				if (savingFlag)
					return false;
				foreach (FilePath file in GetItemFiles (false))
					if (GetLastReloadCheckTime (file) != GetLastWriteTime (file))
						return true;
				return false;
			}
			set {
				reloadCheckTime.Clear ();
				foreach (FilePath file in GetItemFiles (false)) {
					if (value)
						reloadCheckTime [file] = DateTime.MinValue;
					else
						reloadCheckTime [file] = GetLastWriteTime (file);
				}
			}
		}
		
		public virtual bool ItemFilesChanged {
			get {
				if (savingFlag)
					return false;
				foreach (FilePath file in GetItemFiles (false))
					if (GetLastSaveTime (file) != GetLastWriteTime (file))
						return true;
				return false;
			}
		}

		DateTime GetLastWriteTime (FilePath file)
		{
			try {
				if (!file.IsNullOrEmpty && File.Exists (file))
					return File.GetLastWriteTime (file);
			} catch {
			}
			return GetLastSaveTime (file);
		}

		DateTime GetLastSaveTime (FilePath file)
		{
			DateTime dt;
			if (lastSaveTime.TryGetValue (file, out dt))
				return dt;
			else
				return DateTime.MinValue;
		}

		DateTime GetLastReloadCheckTime (FilePath file)
		{
			DateTime dt;
			if (reloadCheckTime.TryGetValue (file, out dt))
				return dt;
			else
				return DateTime.MinValue;
		}
		
		internal protected virtual BuildResult OnRunTarget (IProgressMonitor monitor, string target, string configuration)
		{
			if (target == ProjectService.BuildTarget)
				return OnBuild (monitor, configuration);
			else if (target == ProjectService.CleanTarget) {
				OnClean (monitor, configuration);
				return null;
			}
			return null;
		}
		
		protected virtual void OnClean (IProgressMonitor monitor, string configuration)
		{
		}
		
		protected virtual BuildResult OnBuild (IProgressMonitor monitor, string configuration)
		{
			return null;
		}
		
		internal protected virtual void OnExecute (IProgressMonitor monitor, ExecutionContext context, string configuration)
		{
		}
		
		internal protected virtual bool OnGetCanExecute (ExecutionContext context, string configuration)
		{
			return true;
		}
		
		internal protected virtual bool OnGetNeedsBuilding (string configuration)
		{
			return false;
		}
		
		internal protected virtual void OnSetNeedsBuilding (bool val, string configuration)
		{
		}
		
		void ILoadController.BeginLoad ()
		{
			loading++;
			OnBeginLoad ();
		}
		
		void ILoadController.EndLoad ()
		{
			loading--;
			OnEndLoad ();
		}
		
		protected virtual void OnBeginLoad ()
		{
		}
		
		protected virtual void OnEndLoad ()
		{
		}

		public FilePath GetAbsoluteChildPath (FilePath relPath)
		{
			return relPath.ToAbsolute (BaseDirectory);
		}

		public FilePath GetRelativeChildPath (FilePath absPath)
		{
			return absPath.ToRelative (BaseDirectory);
		}
		
		public virtual void Dispose()
		{
			if (extendedProperties != null) {
				foreach (object ob in extendedProperties.Values) {
					IDisposable disp = ob as IDisposable;
					if (disp != null)
						disp.Dispose ();
				}
			}
			if (userProperties != null)
				((IDisposable)userProperties).Dispose ();
		}
		
		protected virtual void OnNameChanged (WorkspaceItemRenamedEventArgs e)
		{
			NotifyModified ();
			if (NameChanged != null)
				NameChanged (this, e);
		}
		
		protected void NotifyModified ()
		{
			OnModified (new WorkspaceItemEventArgs (this));
		}
		
		protected virtual void OnModified (WorkspaceItemEventArgs args)
		{
			if (Modified != null)
				Modified (this, args);
		}
		
		protected virtual void OnSaved (WorkspaceItemEventArgs args)
		{
			if (Saved != null)
				Saved (this, args);
		}
		
		public event EventHandler ConfigurationsChanged;
		public event EventHandler<WorkspaceItemRenamedEventArgs> NameChanged;
		public event EventHandler<WorkspaceItemEventArgs> Modified;
		public event EventHandler<WorkspaceItemEventArgs> Saved;
	}
}
