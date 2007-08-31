//
// PropertyService.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MonoDevelop.Core
{
	public static class PropertyService
	{
		readonly static string FileName = "MonoDevelopProperties.xml";
		static Properties properties;
		
		public static string EntryAssemblyPath {
			get {
				if (Assembly.GetEntryAssembly () != null)
					return Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);
				return AppDomain.CurrentDomain.BaseDirectory;
			}
		}
		
		public static string ConfigPath {
			get {
				string configPath = Environment.GetEnvironmentVariable ("XDG_CONFIG_HOME");
				if (String.IsNullOrEmpty (configPath))
					configPath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".config");
				return Path.Combine (configPath, "MonoDevelop");
			}
		}
		
		public static string DataPath {
			get {
				string result = System.Configuration.ConfigurationSettings.AppSettings ["DataDirectory"];
				if (String.IsNullOrEmpty (result)) 
					result = Path.Combine (EntryAssemblyPath, Path.Combine ("..", "data"));
				return result;
			}
		}
		
		static PropertyService ()
		{
			if (!LoadProperties (Path.Combine (ConfigPath, FileName))) {
				if (!LoadProperties (Path.Combine (DataPath, FileName))) 
					properties = new Properties ();
			}
			properties.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) {
				if (PropertyChanged != null)
					PropertyChanged (sender, args);
			};
		}
		
		static bool LoadProperties (string fileName)
		{
			properties = null;
			if (File.Exists (fileName)) {
				properties = Properties.Load (fileName);
			}
			return properties != null;
		}
		
		public static void SaveProperties()
		{
			Debug.Assert (properties != null);
			properties.Save (Path.Combine (ConfigPath, FileName));
		}
		
		public static T Get<T> (string property, T defaultValue)
		{
			return properties.Get (property, defaultValue);
		}
		
		public static T Get<T> (string property)
		{
			return properties.Get<T> (property);
		}
		
		public static void Set (string key, object val)
		{
			properties.Set (key, val);
		}
		
		public static event EventHandler<PropertyChangedEventArgs> PropertyChanged;
	}
}
