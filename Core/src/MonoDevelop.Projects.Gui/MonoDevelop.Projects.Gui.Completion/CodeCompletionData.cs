// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike KrÃ?Â¼ger" email="mike@icsharpcode.net"/>
//     <version value="$version"/>
// </file>

using System;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

using MonoDevelop.Projects.Parser;
using MonoDevelop.Core;
using MonoDevelop.Core.Gui;
using MonoDevelop.Projects.Ambience;
using Ambience_ = MonoDevelop.Projects.Ambience.Ambience;

namespace MonoDevelop.Projects.Gui.Completion
{
	public class CodeCompletionData : ICompletionDataWithMarkup
	{
		string image;
		string text;
		string description;
		string pango_description;
		string documentation;
		string completionString;
		
		IClass cls;	// Used for lazily loading class documentation
		Ambience_ ambience;

		bool convertedDocumentation = false;
		
		public string CompletionString {
			get { return completionString; }
			set { completionString = value; }
		}
		
		
		public int Overloads
		{
			get {
				//return overloads;
				return overload_data.Count;
			}
		}
		
		public string Image
		{
			get {
				return image;
			}
			set {
				image = value;
			}
		}
		
		public string[] Text
		{
			get {
				return new string[] { text };
			}
			set {
				text = value[0];
			}
		}
		public string SimpleDescription
		{
			get {
				return description;
			}
			set {
				description = value;
			}
		}
		
		protected string Documentation {
			get { return documentation; }
			set { documentation = value; }
		}
				
		private string GetDescription (string desc)
		{
			if (documentation == null && cls != null) {
				documentation = cls.Documentation;
			}
			if (documentation == null)
				documentation = string.Empty;
				
			if (desc == null)
				desc = string.Empty;
			
			// don't give a description string, if no documentation or description is provided
			if (desc.Length + documentation.Length == 0) {
				return null;
			}
		
			if (!convertedDocumentation) {
				convertedDocumentation = true;
				try {
					documentation = GetDocumentation (documentation);
				} catch (Exception e) {
					Runtime.LoggingService.Error (e);
				}
			}
		
			return (desc + "\n" + documentation).Trim ();
		}		
		
		public string Description
		{
			get {
				if (description == null && cls != null) {
					description = ambience.Convert (cls);
					if (description == null)
						description = string.Empty;
				}
				return GetDescription (description);		
			}
			set {
				description = value;
			}
		}
		
		public string DescriptionPango
		{
			get {
				if (pango_description == null && cls != null) {
					pango_description = ambience.Convert (cls, ConversionFlags.StandardConversionFlags | ConversionFlags.IncludePangoMarkup);
					if (pango_description == null)
						pango_description = string.Empty;
				}
				return GetDescription (pango_description);				
			}
			set {
				description = value;
			}
		}

		Hashtable overload_data = new Hashtable ();

		public CodeCompletionData[] GetOverloads ()
		{
			return (CodeCompletionData[]) (new ArrayList (overload_data.Values)).ToArray (typeof (CodeCompletionData));
		}

		public void AddOverload (CodeCompletionData overload)
		{
			string desc = overload.SimpleDescription;

			if (desc != description || !overload_data.Contains (desc))
				overload_data[desc] = overload;
		}
		
		public CodeCompletionData ()
		{
		}
		
		public CodeCompletionData (string s, string image)
		{
			description = pango_description = documentation = String.Empty;
			text = s;
			completionString = s;
			this.image = image;
		}
		
		public CodeCompletionData (string s, string image, string description)
		{
			description = pango_description = description; 
			documentation = String.Empty;
			text = s;
			completionString = s;
			this.image = image;
		}
		
		public CodeCompletionData (IClass c, Ambience_ ambience, bool allowInstrinsicNames)
		{
			FillCodeCompletionData (c, ambience, allowInstrinsicNames);
		}
		
		public CodeCompletionData (IMethod method, Ambience_ ambience)
		{
			FillCodeCompletionData (method, ambience);
		}
		
		public CodeCompletionData (IField field, Ambience_ ambience)
		{
			FillCodeCompletionData (field, ambience);
		}
		
		public CodeCompletionData (IProperty property, Ambience_ ambience)
		{
			FillCodeCompletionData (property, ambience);
		}
		
		public CodeCompletionData (IEvent e, Ambience_ ambience)
		{
			FillCodeCompletionData (e, ambience);
		}
		
		public CodeCompletionData (IParameter o, Ambience_ ambience)
		{
			FillCodeCompletionData (o, ambience);
		}
		
		public CodeCompletionData (ILanguageItem item, Ambience_ ambience)
		{
			FillCodeCompletionData (item, ambience);
		}
		
		protected void FillCodeCompletionData (ILanguageItem item, Ambience_ ambience)
		{
			if (item is IClass)
				FillCodeCompletionData ((IClass)item, ambience, false);
			else if (item is IMethod)
				FillCodeCompletionData ((IMethod)item, ambience);
			else if (item is IField)
				FillCodeCompletionData ((IField)item, ambience);
			else if (item is IProperty)
				FillCodeCompletionData ((IProperty)item, ambience);
			else if (item is IEvent)
				FillCodeCompletionData ((IEvent)item, ambience);
			else if (item is IParameter)
				FillCodeCompletionData ((IParameter)item, ambience);
			else
				throw new InvalidOperationException ("Unsupported language item type");
		}
		
		protected void FillCodeCompletionData (IClass c, Ambience_ ambience, bool allowInstrinsicNames)
		{
			cls = c;
			this.ambience = ambience;
			image = Services.Icons.GetIcon(c);
			ConversionFlags flags = allowInstrinsicNames ? ConversionFlags.UseIntrinsicTypeNames : ConversionFlags.None;
			text = ambience.Convert (c, flags | ConversionFlags.ShowGenericParameters);
			completionString = ambience.Convert (c, flags);
		}
		
		protected void FillCodeCompletionData (IMethod method, Ambience_ ambience)
		{
			image  = Services.Icons.GetIcon(method);
			text        = method.Name;
			description = ambience.Convert(method);
			pango_description = ambience.Convert(method, ConversionFlags.StandardConversionFlags | ConversionFlags.IncludePangoMarkup);
			completionString = method.Name;
			documentation = method.Documentation;
		}
		
		protected void FillCodeCompletionData (IField field, Ambience_ ambience)
		{
			image  = Services.Icons.GetIcon(field);
			text        = field.Name;
			description = ambience.Convert(field);
			pango_description = ambience.Convert(field, ConversionFlags.StandardConversionFlags | ConversionFlags.IncludePangoMarkup);
			completionString = field.Name;
			documentation = field.Documentation;
		}
		
		protected void FillCodeCompletionData (IProperty property, Ambience_ ambience)
		{
			image  = Services.Icons.GetIcon(property);
			text        = property.Name;
			description = ambience.Convert(property);
			pango_description  = ambience.Convert(property, ConversionFlags.StandardConversionFlags | ConversionFlags.IncludePangoMarkup);
			completionString = property.Name;
			documentation = property.Documentation;
		}
		
		protected void FillCodeCompletionData (IEvent e, Ambience_ ambience)
		{
			image  = Services.Icons.GetIcon(e);
			text        = e.Name;
			description = ambience.Convert(e);
			pango_description  = ambience.Convert(e, ConversionFlags.StandardConversionFlags | ConversionFlags.IncludePangoMarkup);
			completionString = e.Name;
			documentation = e.Documentation;
		}

		protected void FillCodeCompletionData (IParameter o, Ambience_ ambience)
		{
			image = MonoDevelop.Core.Gui.Stock.Field;
			text  = o.Name;
			description = "";
			pango_description = "";
			completionString = o.Name;
			documentation = "";
		}
		
		public static string GetDocumentation (string doc)
		{
			System.IO.StringReader reader = new System.IO.StringReader("<docroot>" + doc + "</docroot>");
			XmlTextReader xml   = new XmlTextReader(reader);
			StringBuilder ret   = new StringBuilder();
			Regex whitespace    = new Regex(@"(\s|\n)+", RegexOptions.Singleline);
			
			try {
				xml.Read();
				do {
					if (xml.NodeType == XmlNodeType.Element) {
						string elname = xml.Name.ToLower();
						if (elname == "remarks") {
							ret.Append("Remarks:\n");
						} else if (elname == "example") {
							ret.Append("Example:\n");
						} else if (elname == "exception") {
							ret.Append("Exception: " + GetCref(xml["cref"]) + ":\n");
						} else if (elname == "returns") {
							ret.Append("Returns: ");
						} else if (elname == "see") {
							ret.Append(GetCref(xml["cref"]) + xml["langword"]);
						} else if (elname == "seealso") {
							ret.Append("See also: " + GetCref(xml["cref"]) + xml["langword"]);
						} else if (elname == "paramref") {
							ret.Append(xml["name"]);
						} else if (elname == "param") {
							ret.Append(xml["name"].Trim() + ": ");
						} else if (elname == "value") {
							ret.Append("Value: ");
						}
					} else if (xml.NodeType == XmlNodeType.EndElement) {
						string elname = xml.Name.ToLower();
						if (elname == "para" || elname == "param") {
							ret.Append("\n");
						}
					} else if (xml.NodeType == XmlNodeType.Text) {
						ret.Append(whitespace.Replace(xml.Value, " "));
					}
				} while (xml.Read ());
			} catch (Exception ex) {
				Runtime.LoggingService.Error (ex);
				return doc;
			}
			return ret.ToString ();
		}
		
		static string GetCref (string cref)
		{
			if (cref == null)
				return "";
			
			if (cref.Length < 2)
				return cref;
			
			if (cref.Substring(1, 1) == ":")
				return cref.Substring (2, cref.Length - 2);
			
			return cref;
		}
	
	}
}
