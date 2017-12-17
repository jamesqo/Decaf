using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Grammars;
using static CoffeeMachine.Grammars.Java8Parser;

namespace CoffeeMachine
{
    internal class DecafVisitor : Java8BaseVisitor<Unit>
    {
        private readonly BrewOptions _options;
        private readonly StringBuilder _csharp;

        private readonly ITokenStream _tokenStream;
        private readonly IParseTree _tree;

        private int _tokenIndex;
        private IToken _currentToken;

        private readonly HashSet<string> _usings;
        private string _namespace;

        public DecafVisitor(BrewOptions options, ITokenStream tokenStream, IParseTree tree)
        {
            _options = options;
            _csharp = new StringBuilder();
            _usings = new HashSet<string>();

            _tokenStream = tokenStream;
            _tree = tree;
        }

        public string GenerateCSharp()
        {
            Visit(_tree);
            // TODO: Add usings and namespace, plus format the C#.
            return _csharp.ToString();
        }

        private void AppendCSharp(string csharpText, int advanceTokenIndexBy = 1, bool processHiddenTokensBeforeCurrent = true)
        {
            if (processHiddenTokensBeforeCurrent)
            {
                ProcessHiddenTokensBeforeCurrent();
            }

            _csharp.Append(csharpText);
            _tokenIndex += advanceTokenIndexBy;
        }

        private void AppendCSharp(string csharpText, IToken correspondingToken)
        {
            _currentToken = correspondingToken;
            AppendCSharp(csharpText);
        }

        private void AppendCSharp(string csharpText, ITerminalNode node)
        {
            AppendCSharp(csharpText, node.Symbol);
        }

        public override Unit VisitTerminal([NotNull] ITerminalNode node)
        {
            _currentToken = node.Symbol;

            // Reference: https://docs.oracle.com/javase/tutorial/java/nutsandbolts/_keywords.html
            switch (_currentToken.Type)
            {
                // TODO: assert
                case BOOLEAN:
                    AppendCSharp("bool");
                    break;
                case EXTENDS:
                    AppendCSharp(":");
                    break;
                case FINAL:
                    AppendCSharp("readonly");
                    break;
                case IMPLEMENTS:
                    AppendCSharp(":");
                    break;
                // TODO: import
                case INSTANCEOF:
                    AppendCSharp("is");
                    break;
                case NATIVE:
                    AppendCSharp("extern");
                    break;
                // TODO: package
                case SUPER:
                    AppendCSharp("base");
                    break;
                case SYNCHRONIZED:
                    AppendCSharp("lock");
                    break;
                // TODO: throws
                // TODO: transient
                default:
                    AppendCSharp(_currentToken.Text);
                    break;
            }

            return default;
        }

        private void ProcessHiddenTokensBeforeCurrent()
        {
            int start = _tokenIndex;
            int end = _currentToken.TokenIndex;

            for (int i = start; i < end; i++)
            {
                ProcessHiddenToken(_tokenStream.Get(i));
            }
        }

        private void ProcessHiddenToken(IToken hiddenToken)
        {
            Debug.Assert(hiddenToken.Channel == TokenConstants.HiddenChannel);
            Debug.Assert(IsWhitespaceOrComment(hiddenToken));

            AppendCSharp(hiddenToken.Text, processHiddenTokensBeforeCurrent: false);

            bool IsWhitespaceOrComment(IToken token)
            {
                switch (token.Type)
                {
                    case WS:
                    case COMMENT:
                    case LINE_COMMENT:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public override Unit VisitAssertStatementNoMessage([NotNull] AssertStatementNoMessageContext context)
        {
            AppendCSharp("Debug.Assert(", context.GetChild<ITerminalNode>(0));
            Visit(context.GetChild(1));
            AppendCSharp(");", context.GetChild<ITerminalNode>(2));

            return default;
        }

        public override Unit VisitAssertStatementWithMessage([NotNull] AssertStatementWithMessageContext context)
        {
            AppendCSharp("Debug.Assert(", context.GetChild<ITerminalNode>(0));
            Visit(context.GetChild(1));
            AppendCSharp(",", context.GetChild<ITerminalNode>(2));
            Visit(context.GetChild(3));
            AppendCSharp(");", context.GetChild<ITerminalNode>(4));

            return default;
        }
    }
}