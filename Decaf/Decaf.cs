using Antlr4.Runtime;
using CoffeeMachine.Grammars;
using static CoffeeMachine.Grammars.Java8Parser;

namespace CoffeeMachine
{
    public static class Decaf
    {
        public static string Brew(string javaCode, BrewOptions options = null)
        {
            options = options ?? BrewOptions.Default;

            // Create a token stream from the code.
            var inputStream = new AntlrInputStream(javaCode);
            var lexer = new Java8Lexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);

            // Feed the tokens to the parser.
            var parser = new Java8Parser(tokenStream);
            // TODO: Make it so that we don't need a full-fledged compilation unit.
            var tree = parser.compilationUnit();

            return new DecafVisitor(options, tokenStream, tree).GenerateCSharp();
        }
    }
}