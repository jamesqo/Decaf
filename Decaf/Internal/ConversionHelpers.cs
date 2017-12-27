using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeMachine.Internal.Diagnostics;
using static CoffeeMachine.Internal.Grammars.Java8GrammarHelpers;
using static CoffeeMachine.Internal.Grammars.Java8Parser;

namespace CoffeeMachine.Internal
{
    internal static class ConversionHelpers
    {
        private static readonly Dictionary<string, string> s_camelCaseToPascalCaseMap = new Dictionary<string, string>
        {
            ["io"] = "IO"
        };

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

        /// <summary>
        /// Gets the name of the Java package that contains a type.
        /// </summary>
        public static string GetPackageName(string javaTypeName)
        {
            D.AssertTrue(!string.IsNullOrEmpty(javaTypeName));

            string[] parts = javaTypeName.Split('.');
            D.AssertTrue(parts.Length > 0);
            Array.Resize(ref parts, parts.Length - 1);
            return string.Join('.', parts);
        }

        /// <summary>
        /// Gets the unqualified name of a Java type.
        /// </summary>
        public static string GetUnqualifiedTypeName(string javaTypeName)
        {
            int dotIndex = javaTypeName.LastIndexOf('.');
            return dotIndex == -1 ? javaTypeName : javaTypeName.Substring(dotIndex + 1);
        }

        public static bool TryConvertToCSharpSpecialType(string javaTypeName, out string csharpTypeName)
        {
            javaTypeName = GetUnqualifiedTypeName(javaTypeName);
            switch (javaTypeName)
            {
                case "boolean":
                case "Boolean":
                    csharpTypeName = "bool";
                    break;
                case "byte":
                case "Byte":
                    csharpTypeName = "byte";
                    break;
                case "char":
                case "Character":
                    csharpTypeName = "char";
                    break;
                case "double":
                case "Double":
                    csharpTypeName = "double";
                    break;
                case "float":
                case "Float":
                    csharpTypeName = "float";
                    break;
                case "int":
                case "Integer":
                    csharpTypeName = "int";
                    break;
                case "long":
                case "Long":
                    csharpTypeName = "long";
                    break;
                case "short":
                case "Short":
                    csharpTypeName = "short";
                    break;
                default:
                    csharpTypeName = default;
                    return false;
            }

            return true;
        }

        private static string ConvertToPascalCase(string camelCase)
        {
            if (s_camelCaseToPascalCaseMap.TryGetValue(camelCase, out string pascalCase))
            {
                return pascalCase;
            }

            char newFirstChar = char.ToUpperInvariant(camelCase[0]);
            return newFirstChar + camelCase.Substring(1);
        }
    }
}