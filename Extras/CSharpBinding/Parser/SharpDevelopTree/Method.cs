// created on 06.08.2003 at 12:35
using System;
using MonoDevelop.Projects.Parser;
using ICSharpCode.NRefactory.Parser;
using ModifierFlags = ICSharpCode.NRefactory.Parser.AST.Modifier;

namespace CSharpBinding.Parser.SharpDevelopTree
{
	public class Method : DefaultMethod
	{
		public Method (string name, ReturnType type, ModifierFlags m, IRegion region, IRegion bodyRegion)
		{
			Name = name;
			returnType = type;
			this.region     = region;
			this.bodyRegion = bodyRegion;
			modifiers = (ModifierEnum)m;
		}
	}
}
