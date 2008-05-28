//
// DomType.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;

namespace MonoDevelop.Projects.Dom
{
	public class DomType : AbstractMember, IType
	{
		protected object sourceProject;
		protected ICompilationUnit compilationUnit;
		protected IReturnType baseType;
		protected List<TypeParameter> typeParameters = new List<TypeParameter> ();
		protected List<IMember> members = new List<IMember> ();
		protected List<IReturnType> implementedInterfaces = new List<IReturnType> ();
		protected ClassType classType = ClassType.Unknown;
		protected string namesp;
		
		public override string FullName {
			get {
				return !String.IsNullOrEmpty (namesp) ? namesp + "." + name : name;
			}
		}
		
		public string Namespace {
			get {
				return namesp;
			}
		}
		
		public object SourceProject {
			get {
				return sourceProject;
			}
		}

		public ICompilationUnit CompilationUnit {
			get {
				return compilationUnit;
			}
		}
		
		public ClassType ClassType {
			get {
				return classType;
			}
		}
		
		public IReturnType BaseType {
			get {
				return baseType;
			}
			set {
				baseType = value;
			}
		}
		
		public IEnumerable<IReturnType> ImplementedInterfaces {
			get {
				return implementedInterfaces;
			}
		}
		
		public IEnumerable<TypeParameter> TypeParameters {
			get {
				return typeParameters;
			}
		}
		
		public virtual IEnumerable<IMember> Members {
			get {
				return members;
			}
		}
		
		public IEnumerable<IType> InnerTypes {
			get {
				foreach (IMember item in Members)
					if (item is IType)
						yield return (IType)item;
			}
		}

		public IEnumerable<IField> Fields {
			get {
				foreach (IMember item in Members)
					if (item is IField)
						yield return (IField)item;
			}
		}

		public IEnumerable<IProperty> Properties {
			get {
				foreach (IMember item in Members)
					if (item is IProperty)
						yield return (IProperty)item;
			}
		}

		public IEnumerable<IMethod> Methods {
			get {
				foreach (IMember item in Members)
					if (item is IMethod)
						yield return (IMethod)item;
			}
		}

		public IEnumerable<IEvent> Events {
			get {
				foreach (IMember item in Members)
					if (item is IEvent)
						yield return (IEvent)item;
			}
		}
		
		static string[,] iconTable = new string[,] {
			{Stock.Error,     Stock.Error,            Stock.Error,              Stock.Error},             // unknown
			{Stock.Class,     Stock.PrivateClass,     Stock.ProtectedClass,     Stock.InternalClass},     // class
			{Stock.Enum,      Stock.PrivateEnum,      Stock.ProtectedEnum,      Stock.InternalEnum},      // enum
			{Stock.Interface, Stock.PrivateInterface, Stock.ProtectedInterface, Stock.InternalInterface}, // interface
			{Stock.Struct,    Stock.PrivateStruct,    Stock.ProtectedStruct,    Stock.InternalStruct},    // struct
			{Stock.Delegate,  Stock.PrivateDelegate,  Stock.ProtectedDelegate,  Stock.InternalDelegate}   // delegate
		};
		
		public override string StockIcon {
			get {
				return iconTable[(int)ClassType, ModifierToOffset (Modifiers)];
			}
		}
		
		protected DomType ()
		{
		}
		
		public DomType (ICompilationUnit compilationUnit, ClassType classType, string name, DomLocation location, string namesp, DomRegion region, List<IMember> members)
		{
			this.compilationUnit = compilationUnit;
			this.classType   = classType;
			this.name        = name;
			this.namesp      = namesp;
			this.bodyRegion  = region;
			this.members     = members;
			this.location    = location;
			
			foreach (IMember member in members) {
				member.DeclaringType = this;
			}
		}
		
		public DomType (ICompilationUnit compilationUnit, 
		                ClassType classType, 
		                Modifiers modifiers,
		                string name, 
		                DomLocation location, 
		                string namesp, 
		                DomRegion region)
		{
			this.compilationUnit = compilationUnit;
			this.classType   = classType;
			this.modifiers   = modifiers;
			this.name        = name;
			this.namesp      = namesp;
			this.bodyRegion  = region;
			this.members     = members;
			this.location    = location;
		}
		
		public static DomType CreateDelegate (ICompilationUnit compilationUnit, string name, DomLocation location, IReturnType type, List<IParameter> parameters)
		{
			DomType result = new DomType ();
			result.compilationUnit = compilationUnit;
			result.name = name;
			result.classType = MonoDevelop.Projects.Dom.ClassType.Delegate;
			result.members.Add (new DomMethod ("Invoke", Modifiers.None, false, location, DomRegion.Empty, type, parameters));
			return result;
		}
		
		public void Add (IMember member)
		{
			this.members.Add (member);
		}
		
		public void AddInterfaceImplementation (IReturnType interf)
		{
			implementedInterfaces.Add (interf);
		}
		
		public override object AcceptVisitior (IDomVisitor visitor, object data)
		{
			return visitor.Visit (this, data);
		}
	}
	
	internal sealed class Stock 
	{
		public static readonly string Error = "gtk-error";
		public static readonly string Class = "md-class";
		public static readonly string Enum = "md-enum";
		public static readonly string Event = "md-event";
		public static readonly string Field = "md-field";
		public static readonly string Interface = "md-interface";
		public static readonly string Method = "md-method";
		public static readonly string Property = "md-property";
		public static readonly string Struct = "md-struct";
		public static readonly string Delegate = "md-delegate";
		
		public static readonly string InternalClass = "md-internal-class";
		public static readonly string InternalDelegate = "md-internal-delegate";
		public static readonly string InternalEnum = "md-internal-enum";
		public static readonly string InternalEvent = "md-internal-event";
		public static readonly string InternalField = "md-internal-field";
		public static readonly string InternalInterface = "md-internal-interface";
		public static readonly string InternalMethod = "md-internal-method";
		public static readonly string InternalProperty = "md-internal-property";
		public static readonly string InternalStruct = "md-internal-struct";
		
		public static readonly string PrivateClass = "md-private-class";
		public static readonly string PrivateDelegate = "md-private-delegate";
		public static readonly string PrivateEnum = "md-private-enum";
		public static readonly string PrivateEvent = "md-private-event";
		public static readonly string PrivateField = "md-private-field";
		public static readonly string PrivateInterface = "md-private-interface";
		public static readonly string PrivateMethod = "md-private-method";
		public static readonly string PrivateProperty = "md-private-property";
		public static readonly string PrivateStruct = "md-private-struct";
		
		public static readonly string ProtectedClass = "md-protected-class";
		public static readonly string ProtectedDelegate = "md-protected-delegate";
		public static readonly string ProtectedEnum = "md-protected-enum";
		public static readonly string ProtectedEvent = "md-protected-event";
		public static readonly string ProtectedField = "md-protected-field";
		public static readonly string ProtectedInterface = "md-protected-interface";
		public static readonly string ProtectedMethod = "md-protected-method";
		public static readonly string ProtectedProperty = "md-protected-property";
		public static readonly string ProtectedStruct = "md-protected-struct";
		
	}
}	
