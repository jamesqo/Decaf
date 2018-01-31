namespace CoffeeMachine.JParser
{
    public class JParseOptions
    {
        internal static JParseOptions Default { get; } = new JParseOptions();

        public JCodeKind ParseAs { get; set; } = JCodeKind.Infer;
    }
}