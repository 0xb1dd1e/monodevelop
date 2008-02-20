//
// ProjectFileNodeBuilder.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Collections;

using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Dialogs;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core.Gui;
using MonoDevelop.Ide.Codons;

namespace MonoDevelop.Ide.Gui.Pads.ProjectPad
{
	public class ProjectFileNodeBuilder: TypeNodeBuilder
	{
		public override Type NodeDataType {
			get { return typeof(ProjectFile); }
		}
		
		public override Type CommandHandlerType {
			get { return typeof(ProjectFileNodeCommandHandler); }
		}
		
		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			return Path.GetFileName (((ProjectFile)dataObject).Name);
		}
		
		public override string ContextMenuAddinPath {
			get { return "/MonoDevelop/Ide/ContextMenu/ProjectPad/ProjectFile"; }
		}
		
		public override void GetNodeAttributes (ITreeNavigator treeNavigator, object dataObject, ref NodeAttributes attributes)
		{
			attributes |= NodeAttributes.AllowRename;
		}
		
		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Gdk.Pixbuf icon, ref Gdk.Pixbuf closedIcon)
		{
			ProjectFile file = (ProjectFile) dataObject;

			label = Path.GetFileName (file.FilePath);
			if (!File.Exists (file.FilePath)) {
				label = "<span foreground='red'>" + label + "</span>";
			}

			string ic = Services.Icons.GetImageForFile (file.FilePath);
			if (ic != Stock.MiscFiles || !File.Exists (file.FilePath))
				icon = Context.GetIcon (ic);
			else
				icon = IdeApp.Services.PlatformService.GetPixbufForFile (file.FilePath, 16);
		}
		
		public override object GetParentObject (object dataObject)
		{
			ProjectFile file = (ProjectFile) dataObject;
			if (file.BuildAction == BuildAction.EmbedAsResource)
				return new ResourceFolder (file.Project);
			string dir = Path.GetDirectoryName (file.FilePath);
			if (dir == file.Project.BaseDirectory)
				return file.Project;
			else if (file.IsExternalToProject)
				return new LinkedFilesFolder (file.Project);
			else
			    return new ProjectFolder (dir, file.Project, null);
		}
		
		public override int CompareObjects (ITreeNavigator thisNode, ITreeNavigator otherNode)
		{
			if (otherNode.DataItem is ProjectFolder)
				return 1;
			else
				return DefaultSort;
		}
	}
	
	public class ProjectFileNodeCommandHandler: NodeCommandHandler
	{
		public override void RenameItem (string newName)
		{
			ProjectFile file = CurrentNode.DataItem as ProjectFile;
			string oldPath = file.Name;
			string newPath = Path.Combine (Path.GetDirectoryName (oldPath), newName);
			
			if (oldPath != newPath) {
				try {
					if (FileService.IsValidFileName (newPath)) {
						FileService.RenameFile (oldPath, newName);
						IdeApp.ProjectOperations.SaveCombine();
					}
				} catch (System.IO.IOException) {   // assume duplicate file
					Services.MessageService.ShowError (GettextCatalog.GetString ("File or directory name is already in use. Please choose a different one."));
				} catch (System.ArgumentException) { // new file name with wildcard (*, ?) characters in it
					Services.MessageService.ShowError (GettextCatalog.GetString ("The file name you have chosen contains illegal characters. Please choose a different file name."));
				}
			}
		}
		
		public override void ActivateItem ()
		{
			ProjectFile file = CurrentNode.DataItem as ProjectFile;
			IdeApp.Workbench.OpenDocument (file.FilePath);
		}
		
		public override DragOperation CanDragNode ()
		{
			return DragOperation.Copy | DragOperation.Move;
		}
		
		public override bool CanDropNode (object dataObject, DragOperation operation)
		{
			return dataObject is CombineEntry;
		}
		
		public override void OnNodeDrop (object dataObject, DragOperation operation)
		{
		}
		
		[CommandHandler (FileCommands.OpenContainingFolder)]
		public void OnOpenFolder ()
		{
			ProjectFile file = CurrentNode.DataItem as ProjectFile;
			string path = System.IO.Path.GetDirectoryName (file.FilePath);
			System.Diagnostics.Process.Start ("file://" + path);
		}

		[CommandHandler (EditCommands.Delete)]
		public override void DeleteItem ()
		{
			ProjectFile file = CurrentNode.DataItem as ProjectFile;
			Project project = CurrentNode.GetParentDataItem (typeof(Project), false) as Project;
			
			DeleteFileDialog deleteDialog = new DeleteFileDialog (GettextCatalog.GetString (
				"Are you sure you want to remove file {0} from project {1}?", Path.GetFileName (file.Name), project.Name));
			try {
				bool yes = deleteDialog.Run ();
				if (!yes) return;

				if (!file.IsExternalToProject) {
					ProjectFile[] inFolder = project.ProjectFiles.GetFilesInPath (Path.GetDirectoryName (file.Name));
					if (inFolder.Length == 1 && inFolder [0] == file) {
						// This is the last project file in the folder. Make sure we keep
						// a reference to the folder, so it is not deleted from the tree.
						ProjectFile folderFile = new ProjectFile (Path.GetDirectoryName (file.Name));
						folderFile.Subtype = Subtype.Directory;
						project.ProjectFiles.Add (folderFile);
					}
				}
				
				project.ProjectFiles.Remove (file);
				if (deleteDialog.DeleteFromDisk)
					FileService.DeleteFile (file.Name);
			
				IdeApp.ProjectOperations.SaveProject (project);				
			} finally {
				deleteDialog.Destroy ();
			}
		}
		
		[CommandUpdateHandler (EditCommands.Delete)]
		public void UpdateRemoveItem (CommandInfo info)
		{
			info.Text = GettextCatalog.GetString ("Remove");
		}
		
		[CommandUpdateHandler (ProjectCommands.IncludeInBuild)]
		public void OnUpdateIncludeInBuild (CommandInfo info)
		{
			ProjectFile file = CurrentNode.DataItem as ProjectFile;
			info.Checked = (file.BuildAction == BuildAction.Compile);
		}
		
		[CommandHandler (ProjectCommands.IncludeInBuild)]
		public void OnIncludeInBuild ()
		{
			ProjectFile finfo = CurrentNode.DataItem as ProjectFile;
			if (finfo.BuildAction == BuildAction.Compile) {
				finfo.BuildAction = BuildAction.Nothing;
			} else {
				finfo.BuildAction = BuildAction.Compile;
			}
			IdeApp.ProjectOperations.SaveProject (finfo.Project);
		}
		
		[CommandUpdateHandler (ProjectCommands.IncludeInDeploy)]
		public void OnUpdateIncludeInDeploy (CommandInfo info)
		{
			ProjectFile finfo = CurrentNode.DataItem as ProjectFile;
			info.Checked = finfo.BuildAction == BuildAction.FileCopy;
		}
		
		[CommandHandler (ProjectCommands.IncludeInDeploy)]
		public void OnIncludeInDeploy ()
		{
			ProjectFile finfo = CurrentNode.DataItem as ProjectFile;

			if (finfo.BuildAction == BuildAction.FileCopy) {
				finfo.BuildAction = BuildAction.Nothing;
			} else {
				finfo.BuildAction = BuildAction.FileCopy;
			}
			IdeApp.ProjectOperations.SaveProject (finfo.Project);
		}
		
		[CommandHandler (ViewCommands.OpenWithList)]
		public void OnOpenWith (object ob)
		{
			ProjectFile finfo = CurrentNode.DataItem as ProjectFile;
			((FileViewer)ob).OpenFile (finfo.Name);
		}
		
		[CommandUpdateHandler (ViewCommands.OpenWithList)]
		public void OnOpenWithUpdate (CommandArrayInfo info)
		{
			ProjectFile finfo = CurrentNode.DataItem as ProjectFile;
			FileViewer prev = null; 
			foreach (FileViewer fv in IdeApp.Workbench.GetFileViewers (finfo.Name)) {
				if (prev != null && fv.IsExternal != prev.IsExternal)
					info.AddSeparator ();
				info.Add (fv.Title, fv);
				prev = fv;
			}
		}
	}
}
