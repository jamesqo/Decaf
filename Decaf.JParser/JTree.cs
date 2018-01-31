using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal.Diagnostics;
using CoffeeMachine.JParser.Internal.Antlr;
using CoffeeMachine.JParser.Nodes;

namespace CoffeeMachine.JParser
{
    public class JTree
    {
        private class AntlrPipeline
        {
            public AntlrInputStream InputStream { get; set; }
            public Java8Lexer Lexer { get; set; }
            public CommonTokenStream TokenStream { get; set; }
            public Java8Parser Parser { get; set; }
        }

        internal JTree(JNode root)
        {
            Root = root;
        }

        public JNode Root { get; }

        public static JTree Parse(string text, JParseOptions options = null)
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
                case JCodeKind.Infer:
                    return parser.codeSnippet();
                case JCodeKind.CompilationUnit:
                    return parser.compilationUnit();
                case JCodeKind.ClassBody:
                    return parser.classBodyDeclarations();
                case JCodeKind.MethodBody:
                    return parser.blockStatements();
                case JCodeKind.Expression:
                    return parser.expression();
                default:
                    Debug.Fail($"Unrecognized {nameof(JCodeKind)} value: {options.ParseAs}");
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

        private static JTree ConvertFromAntlr(IParseTree antlrTree)
        {
            return ConverterVisitor.ConvertAntlrTree(antlrTree);
        }
    }
}