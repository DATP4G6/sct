using Sct.Compiler.Syntax;

namespace SocietalConstructionToolTests
{
    public class SctAstComparer
    {
        public bool DeepReferenceDistinct(SctSyntax x, SctSyntax y)
        {
            var xTree = FlattenTree(x);
            var yTree = FlattenTree(y);
            foreach (var xNode in xTree)
            {
                foreach (var yNode in yTree)
                {
                    if (ReferenceEquals(xNode, yNode))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private IEnumerable<SctSyntax> FlattenTree(SctSyntax syntax) =>
            syntax.Children.SelectMany(FlattenTree).Prepend(syntax);
    }
}
