//
// SystemPackage.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace MonoDevelop.Core
{
	public class SystemPackage
	{
		string name;
		string version;
		string description;
		string[] assemblies;
		bool isInternal;
		ClrVersion targetVersion;
		
		internal void Initialize (string name, string version, string description, string[] assemblies, ClrVersion targetVersion, bool isInternal)
		{
			this.isInternal = isInternal;
			this.name = name;
			this.version = version;
			this.assemblies = assemblies;
			this.description = description;
			this.targetVersion = targetVersion;
		}
		
		public string Name {
			get { return name; }
		}
		
		public string Version {
			get { return version; }
		}
		
		public string Description {
			get { return description; }
		}
		
		public ClrVersion TargetVersion {
			get { return targetVersion; }
		}
		
		// The package is part of the mono SDK
		public bool IsCorePackage {
			get { return name == "mono"; }
		}
		
		// The package has been registered by an add-in, and is not installed
		// in the system.
		public bool IsInternalPackage {
			get { return isInternal; }
		}
		
		public string[] Assemblies {	
			get { return assemblies; }
		}
	}
}
