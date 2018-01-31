namespace CoffeeMachine.JParser
{
    public class JParseOptions
    {
        internal static JParseOptions Default { get; } = new JParseOptions();

        public JNodeKind ParseAs { get; set; } = JNodeKind.Infer;
    }
}