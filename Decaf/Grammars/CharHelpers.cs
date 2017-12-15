namespace CoffeeMachine.Grammars
{
    internal static class CharHelpers
    {
        public static bool IsJavaIdentifierPart(int value)
        {
            // Don't care that much.
            return IsJavaIdentifierStart(value) || (value >= '0' && value <= '9');
        }

        public static bool IsJavaIdentifierStart(int value)
        {
            // Don't care that much.
            return (value >= 'a' && value <= 'z') || (value >= 'A' && value <= 'Z') || value == '_';
        }

        public static int ToCodePoint(char high, char low)
        {
            return char.ConvertToUtf32(high, low);
        }
    }
}