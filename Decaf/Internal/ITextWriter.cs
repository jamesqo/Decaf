namespace CoffeeMachine.Internal
{
    internal interface ITextWriter
    {
        string GetText();
        void Write(string text);
    }
}