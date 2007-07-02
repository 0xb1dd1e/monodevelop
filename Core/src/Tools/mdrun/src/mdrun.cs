//
// mdrun.cs
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
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using Mono.Addins;
using Mono.Addins.Setup;
using System.IO;
using System.Collections;

public class MonoDevelopProcessHost
{
	public static int Main (string[] args)
	{
		if (args.Length == 0 || args [0] == "--help") {
			Console.WriteLine ("MonoDevelop Application Runner");
			Console.WriteLine ("Usage: mdtool <applicationId> ... : Runs an application.");
			Console.WriteLine ("       mdtool setup ... : Runs the setup utility.");
			Console.WriteLine ("       mdtool -q : Lists available applications.");
			return 0;
		}
		
		try {
			Runtime.Initialize (false);
			
			if (args [0] == "-q") {
				Console.WriteLine ("Available applications:");
				foreach (IApplicationInfo ainfo in Runtime.ApplicationService.GetApplications ()) {
					Console.Write ("- " + ainfo.Id);
					if (ainfo.Description != null && ainfo.Description.Length > 0)
						Console.WriteLine (": " + ainfo.Description);
					else
						Console.WriteLine ();
				}
				return 0;
			}
			
			string[] newArgs = new string [args.Length - 1];
			Array.Copy (args, 1, newArgs, 0, args.Length - 1);
			if (args [0] != "setup")
				return Runtime.ApplicationService.StartApplication (args[0], newArgs);
			else
				return RunSetup (newArgs);
		} catch (UserException ex) {
			Console.WriteLine (ex.Message);
			return -1;
		} catch (Exception ex) {
			Console.WriteLine (ex);
			return -1;
		} finally {
			try {
				Runtime.Shutdown ();
			} catch {
				// Ignore shutdown exceptions
			}
		}
	}
	
	static int RunSetup (string[] args)
	{
		Console.WriteLine ("MonoDevelop Add-in Setup Utility");
		bool verbose = false;
		foreach (string a in args)
			if (a == "-v")
				verbose = true;
	
		SetupTool setupTool = new SetupTool (AddinManager.Registry);
		setupTool.VerboseOutput = verbose;
		return setupTool.Run (args);
	}
}
