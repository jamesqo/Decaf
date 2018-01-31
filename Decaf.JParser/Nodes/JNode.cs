using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal.Diagnostics;
using CoffeeMachine.JParser.Internal.Antlr;

namespace CoffeeMachine.JParser.Nodes
{
    public abstract class JNode
    {
        // Span? For ToString() use?

        public abstract JNodeKind Kind { get; }

        public JTree Tree { get; }

        public abstract IEnumerable<JNode> GetChildren();

        public abstract void ToString(StringBuilder sb);
    }
}