using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoffeeMachine.Internal
{
    internal static class RoslynHelpers
    {
        public static ClassDeclarationSyntax ParseClassDeclaration(string text, CSharpParseOptions options)
        {
            var compilationUnit = SyntaxFactory.ParseCompilationUnit(text, options: options);
            return compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        }
    }
}