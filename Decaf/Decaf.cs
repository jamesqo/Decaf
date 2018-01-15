using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal;
using CoffeeMachine.Internal.Diagnostics;
using CoffeeMachine.Internal.Grammars;

namespace CoffeeMachine
{
    public static class Decaf
    {
        private class ParsePipeline
        {
            public AntlrInputStream InputStream { get; set; }
            public Java8Lexer Lexer { get; set; }
            public CommonTokenStream TokenStream { get; set; }
            public Java8Parser Parser { get; set; }
        }

        public static string Brew(string javaCode, BrewOptions options = null)
        {
            if (javaCode == null)
            {
                throw new ArgumentNullException(nameof(javaCode));
            }

            options = options ?? BrewOptions.Default;

            var pipeline = BuildPipeline(javaCode);
            var tree = GetTree(pipeline.Parser, options);
            return new DecafVisitor(options, pipeline.TokenStream, tree).GenerateCSharp();
        }

        private static ParsePipeline BuildPipeline(string javaCode)
        {
            var pipeline = new ParsePipeline();
            pipeline.InputStream = new AntlrInputStream(javaCode);
            pipeline.Lexer = new Java8Lexer(pipeline.InputStream);
            pipeline.TokenStream = new CommonTokenStream(pipeline.Lexer);
            pipeline.Parser = new Java8Parser(pipeline.TokenStream);
            return pipeline;
        }

        private static IParseTree GetTree(Java8Parser parser, BrewOptions options)
        {
            switch (options.ParseAs)
            {
                case CodeKind.Infer:
                    return parser.codeSnippet();
                case CodeKind.CompilationUnit:
                    return parser.compilationUnit();
                case CodeKind.ClassBody:
                    return parser.classBodyDeclarations();
                case CodeKind.MethodBody:
                    return parser.blockStatements();
                case CodeKind.Expression:
                    return parser.expression();
                default:
                    D.Fail($"Unrecognized {nameof(CodeKind)} value: {options.ParseAs}");
                    return default;
            }
        }
    }
}