//  Task.cs
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
using System.Collections;
using System.CodeDom.Compiler;
using MonoDevelop.Projects;
using MonoDevelop.Core.Gui;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.Ide.Tasks
{
	public enum TaskType {
		Error,
		Warning,
		Message,
		SearchResult,
		Comment
	}
	
	public class Task 
	{
		TaskPriority priority = TaskPriority.Normal;
		string   description;
		string   fileName;
		TaskType type;
		Project project;
		int      line;
		int      column;

		int      savedLine = -1;
		
		string errorNumber;
		
		public override string ToString ()
		{
			return String.Format ("[Task:File={0}, Line={1}, Column={2}, Type={3}, Priority={4}, Description={5}]",
			                     fileName,
			                     line,
			                     column,
			                     type,
			                     Enum.GetName (typeof (TaskPriority), priority),
			                     description);
		}
		
		public Project Project {
			get {
				return project;
			}
		}
		
		public TaskPriority Priority
		{
			get { return priority; }
			set { priority = value; }
		}
		
		/// <value>
		/// Used for temporarly line changes (e.g. editing a file) to set back to a default
		/// value when not saving the file and reverting the changes.
		/// </value>
		internal int SavedLine {
			get {
				return savedLine;
			}
			set {
				this.savedLine = value;
			}
		}
		
		public int Line {
			get {
				return line;
			}
			set {
				this.line = value;
			}
		}
		
		public int Column {
			get {
				return column;
			}
			set {
				this.column = value;
			}
		}
		
		public string Description {
			get {
				return description;
			}
		}
		
		public string FileName {
			get {
				return fileName;
			}
			set {
				if (System.IO.File.Exists (fileName))
					fileName = System.IO.Path.GetFullPath (value);
				else
					fileName = value;
			}
		}
		
		public TaskType TaskType {
			get {
				return type;
			}
		}

		public string ErrorNumber {
			get {
				return errorNumber;
			}
		}
		
//		public Task (string fileName, string description, int column, int line)
//		{
//			if (fileName != null)
//				type = TaskType.SearchResult;
//			else
//				type = TaskType.Comment;
//			this.fileName    = fileName;
//			this.description = description.Trim();
//			this.column      = column;
//			this.line        = line;
//		}
		
		public Task (string fileName, string description, int column, int line, TaskType type, Project project)
			: this (fileName, description, column, line, type)
		{
			this.project = project;
		}
		
		public Task (string fileName, string description, int column, int line, TaskType type, Project project, TaskPriority priority)
			: this (fileName, description, column, line, type, priority)
		{
			this.project = project;
		}
		
		public Task (string fileName, string description, int column, int line, TaskType type, TaskPriority priority)
			: this (fileName, description, column, line, type)
		{
			this.priority = priority;
		}
		
		public Task (string fileName, string description, int column, int line, TaskType type)
		{
			this.type        = type;
			this.description = description.Trim();
			this.column      = column;
			this.line        = line;
			FileName    = fileName;
		}
		
		public Task (Project project, CompilerError error)
		{
			this.project = project;
			type        = error.IsWarning ? error.ErrorNumber == "COMMENT" ? TaskType.Message : TaskType.Warning : TaskType.Error;
			column      = error.Column;
			line        = error.Line;
			description = error.ErrorText;
			if (error.ErrorNumber != String.Empty && error.ErrorNumber != null)
				description += "(" + error.ErrorNumber + ")";
			FileName    = error.FileName;
			errorNumber = error.ErrorNumber;
		}
		
		public virtual void JumpToPosition()
		{
			if (fileName != null && fileName.Length > 0) {
				IdeApp.Workbench.OpenDocument (fileName, Math.Max (1, line), Math.Max (1, column), true);
			}
		}
	}
}
