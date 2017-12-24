using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal.Diagnostics;

namespace CoffeeMachine.Internal.Grammars
{
    internal static class AntlrHelpers
    {
        public static int DescendantTokenCount(this IParseTree tree)
        {
            switch (tree)
            {
                case ITerminalNode terminal:
                    return 1;
                default:
                    int tokenCount = 0;
                    int childCount = tree.ChildCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        tokenCount += DescendantTokenCount(tree.GetChild(i));
                    }
                    return tokenCount;
            }
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

        public static ITerminalNode GetLeadingToken(this IParseTree tree)
        {
            if (tree is ITerminalNode terminal)
            {
                return terminal;
            }

            return tree.GetChild(0)?.GetLeadingToken();
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