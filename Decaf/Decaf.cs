using System;
using CoffeeMachine.Internal;
using CoffeeMachine.JParser;

namespace CoffeeMachine
{
    public static class Decaf
    {
        public static string Brew(string javaCode, BrewOptions options = null)
        {
            if (javaCode == null)
            {
                throw new ArgumentNullException(nameof(javaCode));
            }

            options = options ?? BrewOptions.Default;

            var tree = JNode.Parse(text, options.GetJParseOptions());
        }
    }
}