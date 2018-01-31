using System;
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

            var tree = JNode.Parse(javaCode, options.GetJParseOptions());
            // TODO
            return null;
        }
    }
}