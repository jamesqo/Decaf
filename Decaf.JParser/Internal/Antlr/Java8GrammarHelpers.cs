using static CoffeeMachine.JParser.Internal.Antlr.Java8Parser;

namespace CoffeeMachine.JParser.Internal.Antlr
{
    internal static class Java8GrammarHelpers
    {
        public static int GetArgumentCount(ArgumentListContext argumentList)
        {
            // argumentList : expression (',' expression)*

            // Because the argumentList rule requires at least one argument, argumentList?
            // (wrapped in argumentListOrNot) is a common pattern in the grammar.
            if (argumentList == null)
            {
                return 0;
            }

            // ChildCount includes the comma nodes, so we have to adjust it to exclude them.
            return (argumentList.ChildCount + 1) / 2;
        }

        public static int GetTypeArgumentCount(TypeArgumentsContext typeArguments)
        {
            // typeArguments : '<' typeArgumentList '>'

            // Because the typeArguments rule requires at least one type argument, typeArguments?
            // (wrapped in typeArgumentsOrNot) is a common pattern in the grammar.
            if (typeArguments == null)
            {
                return 0;
            }

            var typeArgumentList = typeArguments.typeArgumentList();
            // ChildCount includes the comma nodes, so we have to adjust it to exclude them.
            return (typeArgumentList.ChildCount + 1) / 2;
        }
    }
}