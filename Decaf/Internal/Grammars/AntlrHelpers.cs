using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal.Diagnostics;

namespace CoffeeMachine.Internal.Grammars
{
    internal static class AntlrHelpers
    {
        public static int DescendantTokenCount(this ParserRuleContext context)
        {
            var (start, stop) = (context.Start, context.Stop);
            return stop.TokenIndex - start.TokenIndex + 1;
        }

        public static T GetFirstChild<T>(this IParseTree tree)
            where T : IParseTree
        {
            int childCount = tree.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                if (tree.GetChild(i) is T child)
                {
                    return child;
                }
            }

            return default;
        }

        public static ITerminalNode GetFirstToken(this IParseTree tree, int tokenType)
        {
            int childCount = tree.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                if (tree.GetChild(i) is ITerminalNode terminal &&
                    terminal.Symbol?.Type == tokenType)
                {
                    return terminal;
                }
            }

            return null;
        }

        public static void VisitChildrenBefore(this AbstractParseTreeVisitor<Unit> visitor, IParseTree stop, IParseTree parent)
        {
            D.AssertEqual(stop.Parent, parent);

            for (int i = 0; ; i++)
            {
                var child = parent.GetChild(i);
                if (child == stop)
                {
                    return;
                }

                visitor.Visit(child);
            }
        }
    }
}