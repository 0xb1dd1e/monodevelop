//
// AbstractMember.cs
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

namespace MonoDevelop.Ide.Dom
{
	public abstract class AbstractMember : IMember
	{
		protected IReturnType returnType;
		protected List<IReturnType> explicitInterfaces = new List<IReturnType> ();
		
		protected IType  declaringType;
		public IType DeclaringType {
			get {
				return declaringType;
			}
		}
		
		public virtual string FullName {
			get {
				return DeclaringType != null ? DeclaringType.FullName + "." + Name : Name;
			}
		}
		
		
		public IReturnType ReturnType {
			get {
				return returnType;
			}
		}
		
		public IEnumerable<IReturnType> ExplicitInterfaces {
			get {
				return explicitInterfaces;
			}
		}
		protected string name;
		protected DomRegion region;
		protected DomRegion bodyRegion;
		protected Modifiers modifiers;
		protected List<IAttribute> attributes = new List<IAttribute> ();
		
		public string Name {
			get {
				return name;
			}
		}
		
		public DomRegion Region {
			get {
				return region;
			}
		}

		public DomRegion BodyRegion {
			get {
				return bodyRegion;
			}
		}
		
		public Modifiers Modifiers {
			get {
				return modifiers;
			}
		}
		
		public System.Collections.Generic.IEnumerable<IAttribute> Attributes {
			get {
				return attributes;
			}
		}
		
		/// <summary>
		/// This method is used to look up special methods that are connected to
		/// the member (like set/get method for events).
		/// </summary>
		/// <param name="prefix">
		/// A <see cref="System.String"/> for the prefix. For example the property Name has the method set_Name attacehd
		/// and 'set_' is the prefix.
		/// </param>
		/// <returns>
		/// A <see cref="IMethod"/> when the special method is found, null otherwise.
		/// </returns>
		protected IMethod LookupSpecialMethod (string prefix)
		{
			if (DeclaringType == null)
				return null;
			string specialMethodName = prefix + Name;
			foreach (IMethod method in DeclaringType.Methods) {
				if (method.IsSpecialName && method.Name == specialMethodName)
					return method;
			}
			return null;
		}

		
		public virtual int CompareTo (object obj)
		{
			return 0;
		}
		
		#region ModifierAccessors
		public bool IsPrivate { 
			get {
				return (this.Modifiers & Modifiers.Private) == Modifiers.Private;
			}
		}
		public bool IsInternal { 
			get {
				return (this.Modifiers & Modifiers.Internal) == Modifiers.Internal;
			}
		}
		public bool IsProtected { 
			get {
				return (this.Modifiers & Modifiers.Protected) == Modifiers.Protected;
			}
		}
		public bool IsPublic { 
			get {
				return (this.Modifiers & Modifiers.Public) == Modifiers.Public;
			}
		}
		public bool IsProtectedAndInternal { 
			get {
				return (this.Modifiers & Modifiers.ProtectedAndInternal) == Modifiers.ProtectedAndInternal;
			}
		}
		public bool IsProtectedOrInternal { 
			get {
				return (this.Modifiers & Modifiers.ProtectedOrInternal) == Modifiers.ProtectedOrInternal;
			}
		}
		
		public bool IsAbstract { 
			get {
				return (this.Modifiers & Modifiers.Abstract) == Modifiers.Abstract;
			}
		}
		public bool IsVirtual { 
			get {
				return (this.Modifiers & Modifiers.Virtual) == Modifiers.Virtual;
			}
		}
		public bool IsSealed { 
			get {
				return (this.Modifiers & Modifiers.Sealed) == Modifiers.Sealed;
			}
		}
		public bool IsStatic { 
			get {
				return (this.Modifiers & Modifiers.Static) == Modifiers.Static;
			}
		}
		public bool IsOverride { 
			get {
				return (this.Modifiers & Modifiers.Override) == Modifiers.Override;
			}
		}
		public bool IsReadonly { 
			get {
				return (this.Modifiers & Modifiers.Readonly) == Modifiers.Readonly;
			}
		}
		public bool IsConst { 
			get {
				return (this.Modifiers & Modifiers.Const) == Modifiers.Const;
			}
		}
		public bool IsNew { 
			get {
				return (this.Modifiers & Modifiers.New) == Modifiers.New;
			}
		}
		public bool IsPartial { 
			get {
				return (this.Modifiers & Modifiers.Partial) == Modifiers.Partial;
			}
		}
		
		public bool IsExtern { 
			get {
				return (this.Modifiers & Modifiers.Extern) == Modifiers.Extern;
			}
		}
		public bool IsVolatile { 
			get {
				return (this.Modifiers & Modifiers.Volatile) == Modifiers.Volatile;
			}
		}
		public bool IsUnsafe { 
			get {
				return (this.Modifiers & Modifiers.Unsafe) == Modifiers.Unsafe;
			}
		}
		public bool IsOverloads { 
			get {
				return (this.Modifiers & Modifiers.Overloads) == Modifiers.Overloads;
			}
		}
		public bool IsWithEvents { 
			get {
				return (this.Modifiers & Modifiers.WithEvents) == Modifiers.WithEvents;
			}
		}
		public bool IsDefault { 
			get {
				return (this.Modifiers & Modifiers.Default) == Modifiers.Default;
			}
		}
		public bool IsFixed { 
			get {
				return (this.Modifiers & Modifiers.Fixed) == Modifiers.Fixed;
			}
		}
		
		public bool IsSpecialName { 
			get {
				return (this.Modifiers & Modifiers.SpecialName) == Modifiers.SpecialName;
			}
		}
		public bool IsFinal { 
			get {
				return (this.Modifiers & Modifiers.Final) == Modifiers.Final;
			}
		}
		public bool IsLiteral { 
			get {
				return (this.Modifiers & Modifiers.Literal) == Modifiers.Literal;
			}
		}
		#endregion
		
		public abstract object AcceptVisitior (IDomVisitor visitor, object data);
		
	}
}
