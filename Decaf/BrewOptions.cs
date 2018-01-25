using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CoffeeMachine
{
    public class BrewOptions
    {
        public static BrewOptions Default { get; } = new BrewOptions();

        public string AnonymousClassNameFormat { get; set; } = "Anon{Type}";
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public LanguageVersion CSharpLanguageVersion { get; set; } = LanguageVersion.Latest;
        public IndentationStyle IndentationStyle { get; set; } = IndentationStyle.Preserve;
        public CodeKind ParseAs { get; set; } = CodeKind.Infer;
        public int SpacesPerIndent { get; set; } = 4;
        public bool TranslateCollectionTypes { get; set; } = true;
        public bool UnqualifyTypeNames { get; set; } = false;
        public bool UseVarInDeclarations { get; set; } = true;

        internal string FormatAnonymousClassName(string baseClass)
        {
            return AnonymousClassNameFormat.Replace("{Type}", baseClass);
        }

        internal CSharpParseOptions GetCSharpParseOptions()
        {
            return new CSharpParseOptions(
                languageVersion: CSharpLanguageVersion,
                // We want to parse the output as a C# script because that allows for top-level statements/method declarations.
                kind: SourceCodeKind.Script);
        }
    }
}