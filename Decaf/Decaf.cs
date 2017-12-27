using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal;
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

        private static readonly Dictionary<CodeKind, Func<Java8Parser, IParseTree>> s_doParseFuncs = new Dictionary<CodeKind, Func<Java8Parser, IParseTree>>
        {
            [CodeKind.Expression] = p => p.expression(),
            [CodeKind.MethodBody] = p => p.blockStatements(),
            [CodeKind.ClassBody] = p => p.classBodyDeclarations(),
            [CodeKind.CompilationUnit] = p => p.compilationUnit()
        };

        public static string Brew(string javaCode, BrewOptions options = null)
        {
            options = options ?? BrewOptions.Default;

            var (pipeline, tree) = Parse(javaCode, options);
            return new DecafVisitor(options, pipeline.TokenStream, tree).GenerateCSharp();
        }

        private static (ParsePipeline pipeline, IParseTree tree) Parse(string javaCode, BrewOptions options)
        {
            var pipeline = BuildPipeline(javaCode);
            if (options.ParseAs != CodeKind.Infer)
            {
                var doParse = s_doParseFuncs[options.ParseAs];
                return (pipeline, doParse(pipeline.Parser));
            }

            IParseTree bestTree = null;
            int leastNumberOfErrors = int.MaxValue;

            foreach (var doParse in s_doParseFuncs.Values)
            {
                var parser = pipeline.Parser;
                var tree = doParse(parser);
                int numberOfErrors = parser.NumberOfSyntaxErrors;

                if (numberOfErrors < leastNumberOfErrors)
                {
                    bestTree = tree;
                    leastNumberOfErrors = numberOfErrors;

                    if (numberOfErrors == 0)
                    {
                        break;
                    }
                }

                pipeline = BuildPipeline(javaCode);
            }

            return (pipeline, bestTree);
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
    }
}