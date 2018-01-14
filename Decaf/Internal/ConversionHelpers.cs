using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeMachine.Internal.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
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
        /// Converts a Java identifier to a C# identifier.
        /// </summary>
        public static string ConvertIdentifier(string javaIdentifier)
        {
            string csharpIdentifier = javaIdentifier;
            // Don't convert from camelCase to PascalCase here. It's not always desirable, e.g. in the case of
            // local variables or parameters.
            csharpIdentifier = YellCaseToPascalCase(csharpIdentifier);
            csharpIdentifier = EscapeCSharpIdentifier(csharpIdentifier);
            return csharpIdentifier;
        }

        /// <summary>
        /// Converts a Java method name to a C# method name.
        /// </summary>
        public static string ConvertMethodName(string javaMethodName)
        {
            D.AssertTrue(!string.IsNullOrEmpty(javaMethodName));
            D.AssertTrue(!javaMethodName.Contains("."));

            return CamelCaseToPascalCase(javaMethodName);
        }

        /// <summary>
        /// Converts a Java package name to a C# namespace name.
        /// </summary>
        public static string ConvertPackageName(string packageName)
        {
            string[] parts = packageName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(".", parts.Select(CamelCaseToPascalCase));
        }

        /// <summary>
        /// Converts a Java type name to a C# type name.
        /// </summary>
        public static string ConvertTypeName(string javaTypeName)
        {
            if (TryConvertToCSharpSpecialType(javaTypeName, out string csharpTypeName))
            {
                return csharpTypeName;
            }

            string packageName = GetPackageName(javaTypeName);
            string namespaceName = ConvertPackageName(packageName);
            string csharpUnqualifiedTypeName = YellCaseToPascalCase(GetUnqualifiedTypeName(javaTypeName));

            if (string.IsNullOrEmpty(namespaceName))
            {
                return csharpUnqualifiedTypeName;
            }
            return namespaceName + '.' + csharpUnqualifiedTypeName;
        }

        /// <summary>
        /// Gets the name of the Java package that contains a type.
        /// </summary>
        public static string GetPackageName(string javaTypeName)
        {
            D.AssertTrue(!string.IsNullOrEmpty(javaTypeName));

            string[] parts = javaTypeName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            D.AssertTrue(parts.Length > 0);
            Array.Resize(ref parts, parts.Length - 1);
            return string.Join(".", parts);
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
                case "Object":
                    csharpTypeName = "object";
                    break;
                case "short":
                case "Short":
                    csharpTypeName = "short";
                    break;
                case "String":
                    csharpTypeName = "string";
                    break;
                default:
                    csharpTypeName = default;
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Converts camelCase text to PascalCase text.
        /// </summary>
        private static string CamelCaseToPascalCase(string text)
        {
            if (s_camelCaseToPascalCaseMap.TryGetValue(text, out string pascalCase))
            {
                return pascalCase;
            }

            char newFirstChar = char.ToUpperInvariant(text[0]);
            return newFirstChar + text.Substring(1);
        }

        /// <summary>
        /// Converts YELL_CASE text to PascalCase text.
        /// </summary>
        private static string YellCaseToPascalCase(string text)
        {
            if (!IsYellCase(text))
            {
                return text;
            }

            string[] words = text.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(words.Select(UppercaseToPascalCase));

            string UppercaseToPascalCase(string word)
            {
                D.AssertTrue(IsYellCase(word));

                char[] buffer = word.ToCharArray();
                for (int i = 1; i < word.Length; i++)
                {
                    buffer[i] = char.ToLowerInvariant(buffer[i]);
                }
                return new string(buffer);
            }
        }

        /// <summary>
        /// Escapes a reserved identifier so that it is a valid C# identifier.
        /// </summary>
        private static string EscapeCSharpIdentifier(string identifier)
        {
            return IsCSharpKeyword(identifier) ? '@' + identifier : identifier;
        }

        private static bool IsCSharpKeyword(string text)
        {
            return SyntaxFacts.GetKeywordKind(text) != SyntaxKind.None;
        }

        /// <summary>
        /// Returns whether a string is YELL_CASE.
        /// </summary>
        private static bool IsYellCase(string text)
        {
            foreach (char ch in text)
            {
                // Don't use char.IsUpper because it returns false for digits, but text like 'ROTATION_90'
                // should be recognized as yell-case.
                if (ch != '_' && ch != char.ToUpperInvariant(ch))
                {
                    return false;
                }
            }

            return true;
        }
    }
}