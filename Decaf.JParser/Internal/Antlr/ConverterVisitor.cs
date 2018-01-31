using Antlr4.Runtime.Tree;

namespace CoffeeMachine.JParser.Internal.Antlr
{
    internal class ConverterVisitor : Java8BaseVisitor<Unit>
    {
        private readonly JNode _root;

        private ConverterVisitor()
        {
        }

        public static JNode ConvertAntlrTree(IParseTree tree)
        {
            var visitor = new ConverterVisitor();
            visitor.Visit(tree);
            return visitor._root;
        }
    }
}