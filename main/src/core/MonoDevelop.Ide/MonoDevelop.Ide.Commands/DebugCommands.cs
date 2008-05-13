// DebugCommands.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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
//
//


using System;
using MonoDevelop.Core.Gui.Dialogs;
using MonoDevelop.Core;
using Mono.Addins;
using MonoDevelop.Components;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Components.Commands;

namespace MonoDevelop.Ide.Commands
{
	public enum DebugCommands
	{
		DebugApplication,
		ToggleBreakpoint,
		StepOver,
		StepInto,
		StepOut,
		Pause,
		ClearAllBreakpoints
	}
	
	internal class DebugApplicationHandler: CommandHandler
	{
		protected override void Run ()
		{
			FileSelector fs = new FileSelector (GettextCatalog.GetString ("Application to Debug"));
			try {
				int response = fs.Run ();
				string name = fs.Filename;
				fs.Hide ();
				if (response == (int)Gtk.ResponseType.Ok)
					IdeApp.ProjectOperations.DebugApplication (name);
			}
			finally {
				fs.Destroy ();
			}
		}
		
		protected override void Update (CommandInfo info)
		{
			info.Enabled = IdeApp.ProjectOperations.CurrentRunOperation.IsCompleted;
		}
	}
	
	internal class StepOverHandler : CommandHandler
	{
		protected override void Run ()
		{
			IdeApp.Services.DebuggingService.StepOver();
		}
		
		protected override void Update (CommandInfo info)
		{
			info.Enabled = IdeApp.Services.DebuggingService.IsDebugging && !IdeApp.Services.DebuggingService.IsRunning;
		}
	}

	internal class StepIntoHandler : CommandHandler
	{
		protected override void Run ()
		{
			IdeApp.Services.DebuggingService.StepInto();
		}
		
		protected override void Update (CommandInfo info)
		{
			info.Enabled = IdeApp.Services.DebuggingService.IsDebugging && !IdeApp.Services.DebuggingService.IsRunning;
		}
	}
	
	internal class StepOutHandler : CommandHandler
	{
		protected override void Run ()
		{
			IdeApp.Services.DebuggingService.StepOut ();
		}
		
		protected override void Update (CommandInfo info)
		{
			info.Enabled = IdeApp.Services.DebuggingService.IsDebugging && !IdeApp.Services.DebuggingService.IsRunning;
		}
	}
	
	internal class PauseDebugHandler : CommandHandler
	{
		protected override void Run ()
		{
			IdeApp.Services.DebuggingService.Pause ();
		}
		
		protected override void Update (CommandInfo info)
		{
			info.Enabled = IdeApp.Services.DebuggingService.IsRunning;
		}
	}
	
	internal class ClearAllBreakpointsHandler: CommandHandler
	{
		protected override void Run ()
		{
			IdeApp.Services.DebuggingService.Breakpoints.Clear ();
		}
	}
	
	internal class ToggleBreakpointHandler: CommandHandler
	{
		protected override void Run ()
		{
			IdeApp.Services.DebuggingService.Breakpoints.Toggle (
			    IdeApp.Workbench.ActiveDocument.FileName,
			    IdeApp.Workbench.ActiveDocument.TextEditor.CursorLine);
		}
		
		protected override void Update (CommandInfo info)
		{
			info.Enabled = IdeApp.Workbench.ActiveDocument != null && 
					IdeApp.Workbench.ActiveDocument.TextEditor != null &&
					IdeApp.Workbench.ActiveDocument.FileName != null;
		}
	}
}
