//
// CompilationUnit.cs
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
	public class CompilationUnit : ICompilationUnit
	{
		string fileName;
		
		List<IUsing>     usings         = new List<IUsing> ();
		List<IAttribute> attributes     = new List<IAttribute> ();
		List<IType>      types          = new List<IType> ();
		List<Comment>    comments       = new List<Comment> ();
		List<DomRegion>  foldingRegions = new List<DomRegion> ();
		List<Error>      errors         = new List<Error> ();
		
		public CompilationUnit (string fileName)
		{
			this.fileName = fileName;
		}
		
		#region ICompilationUnit
		string ICompilationUnit.FileName {
			get {
				return fileName;
			}
		}
		
		public IEnumerable<IUsing> Usings {
			get {
				return usings;
			}
		}
		
		public IEnumerable<IAttribute> Attributes {
			get {
				return attributes;
			}
		}
		public int TypeCount {
			get {
				return types.Count;
			}
		}
		public IEnumerable<IType> Types {
			get {
				return types;
			}
		}
		
		public IEnumerable<Comment> Comments {
			get {
				return comments;
			}
		}
		
		public IEnumerable<DomRegion> FoldingRegions {
			get {
				return foldingRegions;
			}
		}
		
		public IEnumerable<Error> Errors {
			get {
				return errors;
			}
		}
		
		object IDomVisitable.AcceptVisitior (IDomVisitor visitor, object data)
		{
			return visitor.Visit (this, data);
		}
		#endregion
		
		public virtual void Dispose ()
		{
		}
		
		public void Add (IUsing newUsing)
		{
			usings.Add (newUsing);
		}
		
		public void Add (IAttribute newAttribute)
		{
			attributes.Add (newAttribute);
		}
		
		public void Add (IType newType)
		{
			types.Add (newType);
		}
		
		public void Add (Comment comment)
		{
			comments.Add (comment);
		}
		
		public void Add (DomRegion domRegion)
		{
			foldingRegions.Add (domRegion);
		}
		
		public void Add (Error error)
		{
			errors.Add (error);
		}
	}
}
