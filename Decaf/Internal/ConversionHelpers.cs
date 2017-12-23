using System;
using CoffeeMachine.Internal.Diagnostics;
using static CoffeeMachine.Internal.Grammars.Java8GrammarHelpers;
using static CoffeeMachine.Internal.Grammars.Java8Parser;

namespace CoffeeMachine.Internal
{
    internal static class ConversionHelpers
    {
        public static bool ConvertGetterInvocation(
            string javaMethodName,
            ArgumentListContext argumentList,
            TypeArgumentsContext typeArguments,
            out string csharpPropertyName)
        {
            csharpPropertyName = null;

            if (javaMethodName.Length <= 3 ||
                !javaMethodName.StartsWith("get", StringComparison.OrdinalIgnoreCase) ||
                GetArgumentCount(argumentList) != 0 ||
                GetTypeArgumentCount(typeArguments) != 0)
            {
                return false;
            }

            csharpPropertyName = ConvertToCamelCase(javaMethodName.Substring(3));
            return true;
        }

        public static bool ConvertSetterInvocation(
            string javaMethodName,
            ArgumentListContext argumentList,
            TypeArgumentsContext typeArguments,
            out string csharpPropertyName)
        {
            csharpPropertyName = null;

            if (javaMethodName.Length <= 3 ||
                !javaMethodName.StartsWith("set", StringComparison.OrdinalIgnoreCase) ||
                GetArgumentCount(argumentList) != 1 ||
                GetTypeArgumentCount(typeArguments) != 0)
            {
                return false;
            }

            csharpPropertyName = ConvertToCamelCase(javaMethodName.Substring(3));
            return true;
        }

        public static string ConvertToCamelCase(string javaMethodName)
        {
            D.AssertTrue(!string.IsNullOrEmpty(javaMethodName));

            char newFirstChar = char.ToUpperInvariant(javaMethodName[0]);
            return newFirstChar + javaMethodName.Substring(1);
        }
    }
}