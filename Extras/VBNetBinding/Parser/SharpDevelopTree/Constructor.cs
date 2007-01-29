// created on 06.08.2003 at 12:35

using MonoDevelop.Projects.Parser;
using ICSharpCode.NRefactory.Parser.AST;

namespace VBBinding.Parser.SharpDevelopTree
{
	public class Constructor : DefaultMethod
	{
		public Constructor(Modifier m, IRegion region, IRegion bodyRegion)
		{
			Name = "#ctor";
			this.region     = region;
			this.bodyRegion = bodyRegion;
			modifiers = (ModifierEnum)m;
		}
	}
}
