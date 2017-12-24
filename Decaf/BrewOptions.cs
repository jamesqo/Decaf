using System.Threading;

namespace CoffeeMachine
{
    public class BrewOptions
    {
        public static BrewOptions Default { get; } = new BrewOptions();

        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public bool IndentWithSpaces { get; set; } = true;
        public int SpacesPerIndent { get; set; } = 4;
        public bool TranslateCollectionTypes { get; set; } = true;
        public bool UseVarInDeclarations { get; set; } = true;
    }
}