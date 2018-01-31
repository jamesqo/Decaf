using Antlr4.Runtime.Tree;

namespace CoffeeMachine.JParser.Internal.Antlr
{
    internal class ConverterVisitor : Java8BaseVisitor<Unit>
    {
        private readonly JNode _root;

        private ConverterVisitor(IParseTree tree)
        {
        }

        public static JTree ConvertAntlrTree(IParseTree tree)
        {
            var visitor = new ConverterVisitor(tree);
            visitor.Visit(tree);
            return new JTree(visitor._root);
        }
    }
}