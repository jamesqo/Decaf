using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CoffeeMachine
{
    public class BrewOptions
    {
        public static BrewOptions Default { get; } = new BrewOptions();

        public string AnonymousClassNameFormat { get; set; } = "Anon{0}";
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public LanguageVersion CSharpLanguageVersion { get; set; } = LanguageVersion.Latest;
        public bool IndentWithSpaces { get; set; } = true;
        public CodeKind ParseAs { get; set; } = CodeKind.Infer;
        public int SpacesPerIndent { get; set; } = 4;
        public bool TranslateCollectionTypes { get; set; } = true;
        public bool UseVarInDeclarations { get; set; } = true;

        internal string FormatAnonymousClassName(string baseClass)
        {
            return string.Format(AnonymousClassNameFormat, baseClass);
        }

        internal CSharpParseOptions GetCSharpParseOptions()
        {
            return new CSharpParseOptions(
                languageVersion: CSharpLanguageVersion,
                kind: SourceCodeKind.Script);
        }
    }
}