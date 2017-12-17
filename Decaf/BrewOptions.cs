namespace CoffeeMachine
{
    public class BrewOptions
    {
        public static BrewOptions Default { get; } = new BrewOptions();

        public bool UseVarInDeclarations { get; set; } = true;
        public bool TranslateCollectionTypes { get; set; } = true;
        public bool IndentWithSpaces { get; set; } = true;
        public int SpacesPerIndent { get; set; } = 4;
    }
}