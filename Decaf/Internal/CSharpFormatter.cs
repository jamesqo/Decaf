using System;
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
            var parseOptions = options.GetCSharpParseOptions();
            var tree = CSharpSyntaxTree.ParseText(csharpCode, parseOptions);
            var root = tree.GetCompilationUnitRoot(options.CancellationToken);

            root = AddClasses(root, state.Classes, parseOptions);
            root = AddNamespace(root, state.Namespace);
            root = AddUsings(root, state.Usings, state.UsingStatics);
            root = root.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation).NormalizeWhitespace();
            return root.ToFullString();
        }

        private static CompilationUnitSyntax AddClasses(CompilationUnitSyntax root, Dictionary<string, string> classes, CSharpParseOptions options)
        {
            return root.AddMembers(classes.Select(CreateClassDeclaration).ToArray());

            ClassDeclarationSyntax CreateClassDeclaration(KeyValuePair<string, string> pair)
            {
                var (className, classBody) = pair;
                return RoslynHelpers.ParseClassDeclaration($"class {className} {classBody}", options);
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

            NamespaceDeclarationSyntax CreateNamespaceDeclaration(string name, SyntaxList<MemberDeclarationSyntax> members)
            {
                return SyntaxFactory.NamespaceDeclaration(
                    SyntaxFactory.IdentifierName(name),
                    externs: default,
                    usings: default,
                    members);
            }
        }

        private static CompilationUnitSyntax AddUsings(CompilationUnitSyntax root, HashSet<string> usings, HashSet<string> usingStatics)
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