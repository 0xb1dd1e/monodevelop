//  ProjectDescriptor.cs
//
//  This file was derived from a file from #Develop. 
//
//  Copyright (C) 2001-2007 Mike Krüger <mkrueger@novell.com>
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Core.Gui;
using MonoDevelop.Core.ProgressMonitoring;

using Project_ = MonoDevelop.Projects.Project;

namespace MonoDevelop.Ide.Templates
{
	/// <summary>
	/// This class is used inside the combine templates for projects.
	/// </summary>
	internal class ProjectDescriptor: ICombineEntryDescriptor
	{
		string name;
		string projectType;
		
		ArrayList files      = new ArrayList(); // contains FileTemplate classes
		ArrayList references = new ArrayList(); 
		ArrayList resources = new ArrayList ();
		
		XmlElement projectOptions = null;
		
		#region public properties
		public ArrayList Files {
			get {
				return files;
			}
		}

		public ArrayList References {
			get {
				return references;
			}
		}

		public ArrayList Resources {
			get {
				return resources;
			}
		}

		public XmlElement ProjectOptions {
			get {
				return projectOptions;
			}
		}
		#endregion

		protected ProjectDescriptor(string name, string relativePath)
		{
			this.name = name;
		}
		
		public string CreateEntry (ProjectCreateInformation projectCreateInformation, string defaultLanguage)
		{
			
			if (projectOptions.GetAttribute ("language") == "") {
/*				if (defaultLanguage == null || defaultLanguage == "")
					throw new InvalidOperationException ("Language not specified in template");
*/				projectOptions.SetAttribute ("language", defaultLanguage);
			}
			
			Project_ project = Services.ProjectService.CreateProject (projectType, projectCreateInformation, projectOptions);
			
			if (project == null) {
				Services.MessageService.ShowError (GettextCatalog.GetString ("Can't create project with type : {0}", projectType));
				return String.Empty;
			}
			
			string newProjectName = StringParserService.Parse(name, new string[,] { 
				{"ProjectName", projectCreateInformation.ProjectName}
			});
			
			project.FileName = Path.Combine (projectCreateInformation.ProjectBasePath, newProjectName + ".mdp");
			project.Name = newProjectName;
			
			// Add References
			foreach (ProjectReference projectReference in references) {
				project.ProjectReferences.Add(projectReference);
			}

			foreach (FileDescriptionTemplate file in resources) {
				SingleFileDescriptionTemplate singleFile = file as SingleFileDescriptionTemplate;
				if (singleFile == null)
					throw new InvalidOperationException ("Only single-file templates can be used to generate resource files");

				try {
					string fileName = singleFile.SaveFile (project, defaultLanguage, project.BaseDirectory, null);

					ProjectFile resource = new ProjectFile (fileName);
					resource.BuildAction = BuildAction.EmbedAsResource;
					project.ProjectFiles.Add(resource);
				} catch (Exception ex) {
					Services.MessageService.ShowError (ex, GettextCatalog.GetString ("File {0} could not be written.", file.Name));
				}
			}
	
			// Add Files
			foreach (FileDescriptionTemplate file in files) {
				try {
					file.AddToProject (project, defaultLanguage, project.BaseDirectory, null);
				} catch (Exception ex) {
					Services.MessageService.ShowError (ex, GettextCatalog.GetString ("File {0} could not be written.", file.Name));
				}
			}
			
			// Save project
			
			using (IProgressMonitor monitor = new NullProgressMonitor ()) {
				if (File.Exists (project.FileName)) {
					if (Services.MessageService.AskQuestion (GettextCatalog.GetString (
						"Project file {0} already exists. Do you want to overwrite\nthe existing file?", project.FileName),
						 GettextCatalog.GetString ("File already exists"))) {
						project.Save (project.FileName, monitor);
					}
				} else {
					project.Save (monitor);
				}
			}
			
			return project.FileName;
		}
		
		public static ProjectDescriptor CreateProjectDescriptor(XmlElement element)
		{
			ProjectDescriptor projectDescriptor = new ProjectDescriptor(element.Attributes["name"].InnerText, element.Attributes["directory"].InnerText);
			
			projectDescriptor.projectType = element.GetAttribute ("type");
			if (projectDescriptor.projectType == "") projectDescriptor.projectType = "DotNet";
			
			projectDescriptor.projectOptions = element["Options"];
			if (projectDescriptor.projectOptions == null)
				projectDescriptor.projectOptions = element.OwnerDocument.CreateElement ("Options");
			
			if (element["Files"] != null) {
				foreach (XmlNode node in element["Files"].ChildNodes) {
					XmlElement elem = node as XmlElement;
					if (elem != null)
						projectDescriptor.files.Add (FileDescriptionTemplate.CreateTemplate (elem));
				}
			}
			if (element["Resources"] != null) {
				foreach (XmlNode node in element["Resources"].ChildNodes) {
					XmlElement elem = node as XmlElement;
					if (elem != null)
						projectDescriptor.resources.Add (FileDescriptionTemplate.CreateTemplate (elem));
				}
			}
			if (element["References"] != null) {
				foreach (XmlNode node in element["References"].ChildNodes) {
					if (node != null && node.Name == "Reference") {
					
						ReferenceType referenceType = (ReferenceType)Enum.Parse(typeof(ReferenceType), node.Attributes["type"].InnerXml);
						ProjectReference projectReference = new ProjectReference (referenceType, node.Attributes["refto"].InnerXml);
						projectDescriptor.references.Add(projectReference);
					}
				}
			}
			return projectDescriptor;
		}
	}
}
