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
    }
}