﻿using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal.Diagnostics;

namespace CoffeeMachine.Internal.Grammars
{
    internal static class AntlrHelpers
    {
        public static int DescendantTokenCount(this ITerminalNode node) => 1;

        public static int DescendantTokenCount(this ParserRuleContext context)
        {
            if (context.ChildCount == 0)
            {
                return 0;
            }

            var (start, stop) = (context.Start, context.Stop);
            return stop.TokenIndex - start.TokenIndex + 1;
        }

        public static int FindChild(this IParseTree parent, IParseTree child)
        {
            D.AssertEqual(child.Parent, parent);

            int childCount = parent.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                if (child == parent.GetChild(i))
                {
                    return i;
                }
            }

            return -1;
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

        public static void VisitChildrenAfter(this AbstractParseTreeVisitor<Unit> visitor, IParseTree start, IParseTree parent)
        {
            D.AssertEqual(start.Parent, parent);

            int childCount = parent.ChildCount;
            for (int i = parent.FindChild(start) + 1; i < childCount; i++)
            {
                visitor.Visit(parent.GetChild(i));
            }
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