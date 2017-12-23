using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal.Diagnostics;
using CoffeeMachine.Internal.Grammars;
using static CoffeeMachine.Internal.ConversionHelpers;
using static CoffeeMachine.Internal.Grammars.Java8Parser;

namespace CoffeeMachine.Internal
{
    internal class DecafVisitor : Java8BaseVisitor<Unit>
    {
        private readonly BrewOptions _options;
        private readonly ITextWriter _csharp;

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
            return _csharp.GetText();
        }

        private void AdvanceTokenIndex(int offset)
        {
            _tokenIndex += offset;
        }

        private void AppendCSharpNoAdvance(string csharpText)
        {
            _csharp.Write(csharpText);
        }

        private void AppendCSharp(string csharpText, int advanceTokenIndexBy = 1, bool processHiddenTokensBeforeCurrent = true)
        {
            if (processHiddenTokensBeforeCurrent)
            {
                ProcessHiddenTokensBeforeCurrent();
            }

            AppendCSharpNoAdvance(csharpText);
            AdvanceTokenIndex(advanceTokenIndexBy);
        }

        private void AppendCSharp(string csharpText, IToken correspondingToken)
        {
            D.AssertNotNull(correspondingToken);

            _currentToken = correspondingToken;
            AppendCSharp(csharpText);
        }

        private void AppendCSharp(string csharpText, ITerminalNode node)
        {
            D.AssertNotNull(node);

            AppendCSharp(csharpText, node.Symbol);
        }

        public override Unit VisitErrorNode(IErrorNode node)
        {
            _currentToken = node.Symbol;
            D.AssertNotNull(_currentToken);

            AppendCSharp(_currentToken.Text);
            return default;
        }

        public override Unit VisitTerminal([NotNull] ITerminalNode node)
        {
            _currentToken = node.Symbol;
            D.AssertNotNull(_currentToken);

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
            AppendCSharp(hiddenToken.Text, processHiddenTokensBeforeCurrent: false);
        }

        public override Unit VisitAssertStatementNoMessage([NotNull] AssertStatementNoMessageContext context)
        {
            // 'assert' expression ';'
            AppendCSharp("Debug.Assert(", context.GetChild<ITerminalNode>(0));
            Visit(context.expression());
            AppendCSharp(");", context.GetChild<ITerminalNode>(2));
            return default;
        }

        public override Unit VisitAssertStatementWithMessage([NotNull] AssertStatementWithMessageContext context)
        {
            // 'assert' expression ':' expression ';'
            AppendCSharp("Debug.Assert(", context.GetChild<ITerminalNode>(0));
            Visit(context.GetChild(1));
            AppendCSharp(",", context.GetChild<ITerminalNode>(2));
            Visit(context.GetChild(3));
            AppendCSharp(");", context.GetChild<ITerminalNode>(4));
            return default;
        }

        public override Unit VisitSimpleMethodInvocation([NotNull] SimpleMethodInvocationContext context)
        {
            // methodName '(' argumentListOrNot ')'
            // methodName : Identifier
            // argumentListOrNot : argumentList?
            var methodNameNode = context.methodName().Identifier();
            string methodName = methodNameNode.GetText();
            var argumentListOrNot = context.argumentListOrNot();
            var argumentList = argumentListOrNot.argumentList();

            if (ConvertGetterInvocation(methodName, argumentList, null, out string getterPropertyName))
            {
                ProcessHiddenTokensBeforeCurrent();
                AppendCSharpNoAdvance(getterPropertyName);
                AdvanceTokenIndex(3); // methodName '(' ')'
                return default;
            }

            if (ConvertSetterInvocation(methodName, argumentList, null, out string setterPropertyName))
            {
                ProcessHiddenTokensBeforeCurrent();
                AppendCSharpNoAdvance(setterPropertyName);
                AppendCSharpNoAdvance("=");
                AdvanceTokenIndex(2); // methodName '('
                Visit(argumentList);
                AdvanceTokenIndex(1); // ')'
                return default;
            }

            string csharpMethodName = ConvertToCamelCase(methodName);
            AppendCSharp(csharpMethodName, methodNameNode);
            Visit(context.GetChild(1));
            Visit(argumentListOrNot);
            Visit(context.GetChild(3));
            return default;
        }

        public override Unit VisitNotSoSimpleMethodInvocation([NotNull] NotSoSimpleMethodInvocationContext context)
        {
            /*
             notSoSimpleMethodInvocation
                :   typeName '.' typeArgumentsOrNot Identifier '(' argumentListOrNot ')'
                |   expressionName '.' typeArgumentsOrNot Identifier '(' argumentListOrNot ')'
                |   primary '.' typeArgumentsOrNot Identifier '(' argumentListOrNot ')'
                |   'super' '.' typeArgumentsOrNot Identifier '(' argumentListOrNot ')'
                |   typeName '.' 'super' '.' typeArgumentsOrNot Identifier '(' argumentListOrNot ')'
    ;           ;
             */

            string methodName = context.Identifier().GetText();
            var typeArgumentsOrNot = context.typeArgumentsOrNot();
            var typeArgumentsNode = typeArgumentsOrNot.typeArguments();
            var argumentListOrNot = context.argumentListOrNot();
            var argumentList = argumentListOrNot.argumentList();
            var lparen = context.GetFirstToken(LPAREN);
            var rparen = context.GetFirstToken(RPAREN);

            for (int i = 0; ; i++)
            {
                var child = context.GetChild(i);
                if (child == typeArgumentsOrNot)
                {
                    break;
                }

                Visit(child);
            }

            if (ConvertGetterInvocation(methodName, argumentList, typeArgumentsNode, out string getterPropertyName))
            {
                ProcessHiddenTokensBeforeCurrent();
                AppendCSharpNoAdvance(getterPropertyName);
                AdvanceTokenIndex(3); // Identifier '(' ')'
                return default;
            }

            if (ConvertSetterInvocation(methodName, argumentList, typeArgumentsNode, out string setterPropertyName))
            {
                ProcessHiddenTokensBeforeCurrent();
                AppendCSharpNoAdvance(setterPropertyName);
                AppendCSharpNoAdvance("=");
                AdvanceTokenIndex(2); // Identifier '('
                Visit(argumentList);
                AdvanceTokenIndex(1); // ')'
                return default;
            }

            string typeArguments = Fork(clone => clone.Visit(typeArgumentsOrNot));

            string csharpMethodName = ConvertToCamelCase(methodName);
            AppendCSharpNoAdvance(csharpMethodName);
            AppendCSharpNoAdvance(typeArguments);
            AdvanceTokenIndex(typeArgumentsOrNot.GetDescendantTokenCount() + 1); // typeArgumentsOrNot Identifier
            Visit(lparen);
            Visit(argumentListOrNot);
            Visit(rparen);
            return default;
        }
    }
}