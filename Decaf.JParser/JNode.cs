using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal.Diagnostics;
using CoffeeMachine.JParser.Internal.Antlr;

namespace CoffeeMachine.JParser
{
    public class JNode
    {
        private class AntlrPipeline
        {
            public AntlrInputStream InputStream { get; set; }
            public Java8Lexer Lexer { get; set; }
            public CommonTokenStream TokenStream { get; set; }
            public Java8Parser Parser { get; set; }
        }

        public static JNode Parse(string text, JParseOptions options = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            options = options ?? JParseOptions.Default;

            var antlrTree = AntlrParse(text, options);
            return ConvertFromAntlr(antlrTree);
        }

        private static IParseTree AntlrParse(string text, JParseOptions options)
        {
            var pl = BuildPipeline(text);
            var parser = pl.Parser;

            switch (options.ParseAs)
            {
                case JNodeKind.Infer:
                    return parser.codeSnippet();
                case JNodeKind.CompilationUnit:
                    return parser.compilationUnit();
                case JNodeKind.ClassBody:
                    return parser.classBodyDeclarations();
                case JNodeKind.MethodBody:
                    return parser.blockStatements();
                case JNodeKind.Expression:
                    return parser.expression();
                default:
                    Debug.Fail($"Unrecognized {nameof(JNodeKind)} value: {options.ParseAs}");
                    return default;
            }
        }

        private static AntlrPipeline BuildPipeline(string text)
        {
            var pl = new AntlrPipeline();
            pl.InputStream = new AntlrInputStream(text);
            pl.Lexer = new Java8Lexer(pl.InputStream);
            pl.TokenStream = new CommonTokenStream(pl.Lexer);
            pl.Parser = new Java8Parser(pl.TokenStream);
            return pl;
        }

        private static JNode ConvertFromAntlr(IParseTree antlrTree)
        {
            return ConverterVisitor.ConvertAntlrTree(antlrTree);
        }
    }
}