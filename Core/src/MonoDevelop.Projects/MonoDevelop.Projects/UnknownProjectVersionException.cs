// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version value="$version"/>
// </file>

using System;
using MonoDevelop.Core;

namespace MonoDevelop.Projects
{
	public class UnknownProjectVersionException : Exception
	{
		public UnknownProjectVersionException (string file, string version)
		: base (GettextCatalog.GetString ("The file '{0}' has an unknown format version (version '{1}')'.", file, version))
		{
		}
	}
	
}
