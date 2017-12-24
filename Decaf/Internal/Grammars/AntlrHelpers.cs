using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal.Diagnostics;

namespace CoffeeMachine.Internal.Grammars
{
    internal static class AntlrHelpers
    {
        public static int GetDescendantTokenCount(this IParseTree tree)
        {
            D.AssertNotNull(tree);

            switch (tree)
            {
                case ITerminalNode terminal:
                    return 1;
                default:
                    int tokenCount = 0;
                    int childCount = tree.ChildCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        tokenCount += GetDescendantTokenCount(tree.GetChild(i));
                    }
                    return tokenCount;
            }
        }

        public static ITerminalNode GetFirstToken(this ParserRuleContext context, int tokenType)
        {
            int childCount = context.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                if (context.GetChild(i) is ITerminalNode terminal &&
                    terminal.Symbol?.Type == tokenType)
                {
                    return terminal;
                }
            }

            return null;
        }
    }
}