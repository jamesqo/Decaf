using System.Collections.Generic;
using System.Linq;
using CoffeeMachine.Internal.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace CoffeeMachine.Internal
{
    internal static class CSharpFormatter
    {
        public static string Format(
            string csharpCode,
            CSharpGlobalState state,
            BrewOptions options)
        {
            var root = ParseCSharp(csharpCode, options);

            if (root is CompilationUnitSyntax cuRoot)
            {
                var parseOptions = options.GetCSharpParseOptions();
                cuRoot = AddClasses(cuRoot, state.Classes, parseOptions);
                cuRoot = AddNamespace(cuRoot, state.Namespace);
                cuRoot = AddUsings(cuRoot, state.Usings, state.UsingStatics);
            }

            root = root.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation).NormalizeWhitespace();
            return root.ToFullString();
        }

        private static SyntaxNode ParseCSharp(string code, BrewOptions options)
        {
            D.AssertTrue(options.ParseAs != CodeKind.Infer, $"Update {nameof(options)} to reflect the kind of code that was parsed.");

            var parseOptions = options.GetCSharpParseOptions();
            switch (options.ParseAs)
            {
                case CodeKind.CompilationUnit:
                case CodeKind.ClassBody:
                case CodeKind.MethodBody:
                    return SyntaxFactory.ParseCompilationUnit(code, options: parseOptions);
                case CodeKind.Expression:
                    return SyntaxFactory.ParseExpression(code, options: parseOptions, consumeFullText: true);
                default:
                    D.Fail($"Unrecognized {nameof(CodeKind)} value: {options.ParseAs}");
                    return default;
            }
        }

        private static CompilationUnitSyntax AddClasses(
            CompilationUnitSyntax root,
            Dictionary<string, CSharpClassInfo> classes,
            CSharpParseOptions options)
        {
            return root.AddMembers(classes.Select(CreateClassDeclaration).ToArray());

            ClassDeclarationSyntax CreateClassDeclaration(KeyValuePair<string, CSharpClassInfo> pair)
            {
                var (name, info) = pair;
                string baseTypes = string.Join(", ", info.BaseTypes);
                string modifiers = string.Join(" ", info.Modifiers);
                string text = $"{modifiers} class {name} : {baseTypes} {info.Body}";
                return RoslynHelpers.ParseClassDeclaration(text, options);
            }
        }

        private static CompilationUnitSyntax AddNamespace(CompilationUnitSyntax root, string @namespace)
        {
            if (string.IsNullOrEmpty(@namespace))
            {
                return root;
            }

            return root.WithMembers(
                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                    CreateNamespaceDeclaration(@namespace, root.Members)));

            NamespaceDeclarationSyntax CreateNamespaceDeclaration(
                string name,
                SyntaxList<MemberDeclarationSyntax> members)
            {
                return SyntaxFactory.NamespaceDeclaration(
                    SyntaxFactory.IdentifierName(name),
                    externs: default,
                    usings: default,
                    members);
            }
        }

        private static CompilationUnitSyntax AddUsings(
            CompilationUnitSyntax root,
            HashSet<string> usings,
            HashSet<string> usingStatics)
        {
            D.AssertTrue(!root.Usings.Any(), "The generated C# code shouldn't have usings right after it is translated from Java.");

            return root
                .AddUsings(usings.Select(CreateUsingDirective).ToArray())
                .AddUsings(usingStatics.Select(CreateUsingStaticDirective).ToArray());

            UsingDirectiveSyntax CreateUsingDirective(string @namespace)
            {
                return SyntaxFactory.UsingDirective(
                    SyntaxFactory.IdentifierName(@namespace));
            }

            UsingDirectiveSyntax CreateUsingStaticDirective(string @namespace)
            {
                return SyntaxFactory.UsingDirective(
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                    alias: null,
                    SyntaxFactory.IdentifierName(@namespace));
            }
        }
    }
}