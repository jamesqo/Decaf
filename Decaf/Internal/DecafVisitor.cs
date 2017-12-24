using System;
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
        private class State
        {
            public IToken CurrentToken { get; set; }
            public StringBuilder Output { get; set; }
            public int TokenIndex { get; set; }

            public State Clone()
            {
                return new State
                {
                    CurrentToken = CurrentToken,
                    Output = Output,
                    TokenIndex = TokenIndex
                };
            }
        }

        private readonly BrewOptions _options;
        private readonly ITokenStream _tokenStream;
        private readonly IParseTree _tree;
        private State _state;

        private readonly HashSet<string> _usings;
        private string _namespace;

        public DecafVisitor(BrewOptions options, ITokenStream tokenStream, IParseTree tree)
        {
            _options = options;
            _tokenStream = tokenStream;
            _tree = tree;
            _state = new State
            {
                Output = new StringBuilder()
            };

            _usings = new HashSet<string>();
        }

        public string GenerateCSharp()
        {
            Visit(_tree);
            // TODO: Add usings and namespace, plus format the C#.
            return _state.Output.ToString();
        }

        #region Private helper methods

        private void AdvanceTokenIndex(int offset)
        {
            _state.TokenIndex += offset;
        }

        private string CaptureOutput(Action action)
        {
            var originalState = _state.Clone();
            _state.Output = new StringBuilder();
            action();
            string result = _state.Output.ToString();
            _state = originalState;
            return result;
        }

        private void ProcessHiddenTokensBeforeCurrent()
        {
            int start = _state.TokenIndex;
            int end = _state.CurrentToken.TokenIndex;

            for (int i = start; i < end; i++)
            {
                ProcessHiddenToken(_tokenStream.Get(i));
            }
        }

        private void ProcessHiddenToken(IToken hiddenToken)
        {
            Write(hiddenToken.Text, processHiddenTokensBeforeCurrent: false);
        }

        private void Write(string csharpText, int advanceTokenIndexBy = 1, bool processHiddenTokensBeforeCurrent = true)
        {
            if (processHiddenTokensBeforeCurrent)
            {
                ProcessHiddenTokensBeforeCurrent();
            }

            WriteNoAdvance(csharpText);
            AdvanceTokenIndex(advanceTokenIndexBy);
        }

        private void Write(string csharpText, IToken correspondingToken)
        {
            D.AssertNotNull(correspondingToken);

            _state.CurrentToken = correspondingToken;
            Write(csharpText);
        }

        private void Write(string csharpText, ITerminalNode node)
        {
            D.AssertNotNull(node);

            Write(csharpText, node.Symbol);
        }

        private void WriteNoAdvance(string csharpText)
        {
            _state.Output.Append(csharpText);
        }

        private bool WriteGetterProperty(
            string javaMethodName,
            ArgumentListContext argumentList,
            TypeArgumentsContext typeArguments)
        {
            if (!ConvertGetterInvocation(javaMethodName, argumentList, typeArguments, out string csharpPropertyName))
            {
                return false;
            }

            ProcessHiddenTokensBeforeCurrent();
            WriteNoAdvance(csharpPropertyName);
            AdvanceTokenIndex(3); // Identifier '(' ')'
            return true;
        }

        private bool WriteSetterProperty(
            string javaMethodName,
            ArgumentListContext argumentList,
            TypeArgumentsContext typeArguments)
        {
            if (!ConvertSetterInvocation(javaMethodName, argumentList, typeArguments, out string csharpPropertyName))
            {
                return false;
            }

            ProcessHiddenTokensBeforeCurrent();
            WriteNoAdvance(csharpPropertyName);
            WriteNoAdvance("=");
            AdvanceTokenIndex(2); // Identifier '('
            Visit(argumentList);
            AdvanceTokenIndex(1); // ')'
            return true;
        }

        #endregion

        #region Visit methods

        public override Unit VisitErrorNode(IErrorNode node)
        {
            _state.CurrentToken = node.Symbol;
            D.AssertNotNull(_state.CurrentToken);

            Write(_state.CurrentToken.Text);
            return default;
        }

        public override Unit VisitTerminal([NotNull] ITerminalNode node)
        {
            _state.CurrentToken = node.Symbol;
            D.AssertNotNull(_state.CurrentToken);

            // Reference: https://docs.oracle.com/javase/tutorial/java/nutsandbolts/_keywords.html
            switch (_state.CurrentToken.Type)
            {
                case BOOLEAN:
                    Write("bool");
                    break;
                case EXTENDS:
                    Write(":");
                    break;
                case FINAL:
                    Write("readonly");
                    break;
                case IMPLEMENTS:
                    Write(":");
                    break;
                // TODO: import
                case INSTANCEOF:
                    Write("is");
                    break;
                case NATIVE:
                    Write("extern");
                    break;
                // TODO: package
                case SUPER:
                    Write("base");
                    break;
                case SYNCHRONIZED:
                    Write("lock");
                    break;
                // TODO: throws
                // TODO: transient
                default:
                    Write(_state.CurrentToken.Text);
                    break;
            }
            return default;
        }

        public override Unit VisitAssertStatementNoMessage([NotNull] AssertStatementNoMessageContext context)
        {
            // 'assert' expression ';'
            Write("Debug.Assert(", (ITerminalNode)context.GetChild(0));
            Visit(context.expression());
            Write(");", (ITerminalNode)context.GetChild(2));
            return default;
        }

        public override Unit VisitAssertStatementWithMessage([NotNull] AssertStatementWithMessageContext context)
        {
            // 'assert' expression ':' expression ';'
            Write("Debug.Assert(", (ITerminalNode)context.GetChild(0));
            Visit(context.GetChild(1));
            Write(",", (ITerminalNode)context.GetChild(2));
            Visit(context.GetChild(3));
            Write(");", (ITerminalNode)context.GetChild(4));
            return default;
        }

        public override Unit VisitSimpleMethodInvocation([NotNull] SimpleMethodInvocationContext context)
        {
            return CommonVisitSimpleMethodInvocation(context);
        }

        public override Unit VisitSimpleMethodInvocation_lfno_primary([NotNull] SimpleMethodInvocation_lfno_primaryContext context)
        {
            return CommonVisitSimpleMethodInvocation(context);
        }

        private Unit CommonVisitSimpleMethodInvocation(ParserRuleContext context)
        {
            // methodName '(' argumentListOrNot ')'
            // methodName : Identifier
            // argumentListOrNot : argumentList?
            var methodNameNode = context.GetFirstChild<MethodNameContext>().Identifier();
            string methodName = methodNameNode.GetText();
            var argumentListOrNot = context.GetFirstChild<ArgumentListOrNotContext>();
            var argumentList = argumentListOrNot.argumentList();

            if (WriteGetterProperty(methodName, argumentList, null) ||
                WriteSetterProperty(methodName, argumentList, null))
            {
                return default;
            }

            string csharpMethodName = ConvertToCamelCase(methodName);
            Write(csharpMethodName, methodNameNode);
            Visit(context.GetChild(1));
            Visit(argumentListOrNot);
            Visit(context.GetChild(3));
            return default;
        }

        public override Unit VisitNotSoSimpleMethodInvocation([NotNull] NotSoSimpleMethodInvocationContext context)
        {
            return CommonVisitNotSoSimpleMethodInvocation(context);
        }

        public override Unit VisitMethodInvocation_lf_primary([NotNull] MethodInvocation_lf_primaryContext context)
        {
            return CommonVisitNotSoSimpleMethodInvocation(context);
        }

        public override Unit VisitNotSoSimpleMethodInvocation_lfno_primary([NotNull] NotSoSimpleMethodInvocation_lfno_primaryContext context)
        {
            return CommonVisitNotSoSimpleMethodInvocation(context);
        }

        private Unit CommonVisitNotSoSimpleMethodInvocation(ParserRuleContext context)
        {
            string methodName = context.GetFirstToken(Identifier).GetText();
            var typeArgumentsOrNot = context.GetFirstChild<TypeArgumentsOrNotContext>();
            var typeArgumentsNode = typeArgumentsOrNot.typeArguments();
            var argumentListOrNot = context.GetFirstChild<ArgumentListOrNotContext>();
            var argumentList = argumentListOrNot.argumentList();

            this.VisitChildrenBefore(typeArgumentsOrNot, context);

            if (WriteGetterProperty(methodName, argumentList, typeArgumentsNode) ||
                WriteSetterProperty(methodName, argumentList, typeArgumentsNode))
            {
                return default;
            }

            var lparen = context.GetFirstToken(LPAREN);
            var rparen = context.GetFirstToken(RPAREN);
            string typeArguments = CaptureOutput(() => Visit(typeArgumentsOrNot));

            string csharpMethodName = ConvertToCamelCase(methodName);
            WriteNoAdvance(csharpMethodName);
            WriteNoAdvance(typeArguments);
            AdvanceTokenIndex(typeArgumentsOrNot.DescendantTokenCount() + 1); // typeArgumentsOrNot Identifier
            Visit(lparen);
            Visit(argumentListOrNot);
            Visit(rparen);
            return default;
        }

        public override Unit VisitThrows_OrNot([NotNull] Throws_OrNotContext context)
        {
            // Exclude checked exceptions from the C# output.
            AdvanceTokenIndex(context.DescendantTokenCount());
            return default;
        }

        #endregion
    }
}