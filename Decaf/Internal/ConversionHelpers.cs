using System;
using System.Linq;
using CoffeeMachine.Internal.Diagnostics;
using static CoffeeMachine.Internal.Grammars.Java8GrammarHelpers;
using static CoffeeMachine.Internal.Grammars.Java8Parser;

namespace CoffeeMachine.Internal
{
    internal static class ConversionHelpers
    {
        /// <summary>
        /// Converts a Java getter method name to a C# property name.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the method is eligible for conversion; otherwise, <see langword="false"/>.
        /// </returns>
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

            csharpPropertyName = ConvertMethodName(javaMethodName.Substring(3));
            return true;
        }

        /// <summary>
        /// Converts a Java setter method name to a C# property name.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the method is eligible for conversion; otherwise, <see langword="false"/>.
        /// </returns>
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

            csharpPropertyName = ConvertMethodName(javaMethodName.Substring(3));
            return true;
        }

        /// <summary>
        /// Converts a Java method name to a C# method name.
        /// </summary>
        public static string ConvertMethodName(string javaMethodName)
        {
            D.AssertTrue(!string.IsNullOrEmpty(javaMethodName));
            D.AssertTrue(!javaMethodName.Contains("."));

            return ConvertToPascalCase(javaMethodName);
        }

        /// <summary>
        /// Converts a Java package name to a C# namespace name.
        /// </summary>
        public static string ConvertPackageName(string javaPackageName)
        {
            D.AssertTrue(!string.IsNullOrEmpty(javaPackageName));

            return string.Join('.', javaPackageName.Split('.').Select(ConvertToPascalCase));
        }

        private static string ConvertToPascalCase(string camelCase)
        {
            char newFirstChar = char.ToUpperInvariant(camelCase[0]);
            return newFirstChar + camelCase.Substring(1);
        }
    }
}