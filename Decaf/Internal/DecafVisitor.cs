using System;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using CoffeeMachine.Internal.Diagnostics;
using CoffeeMachine.Internal.Grammars;
using Microsoft.CodeAnalysis.CSharp;
using static CoffeeMachine.Internal.ConversionHelpers;
using static CoffeeMachine.Internal.Grammars.Java8Parser;

namespace CoffeeMachine.Internal
{
    internal partial class DecafVisitor : Java8BaseVisitor<Unit>
    {
        private class RewindableState
        {
            public StringBuilder Output { get; set; }
            public int TokenIndex { get; set; }

            public RewindableState Clone() => new RewindableState
            {
                Output = Output,
                TokenIndex = TokenIndex
            };

            public void ResetOutput() => Output = new StringBuilder();
        }

        private readonly BrewOptions _options;
        private readonly ITokenStream _tokenStream;
        private readonly IParseTree _tree;
        private readonly CSharpGlobalState _gstate;

        private RewindableState _rstate;

        // These are mutable structs; do not make them readonly nor copy them.
        private Channel<string> _genericMethodHeaderChannel;
        private Channel<bool> _methodAnnotationChannel;
        private Channel<bool> _methodDeclarationChannel;

        public DecafVisitor(BrewOptions options, ITokenStream tokenStream, IParseTree tree)
        {
            _options = options;
            _tokenStream = tokenStream;
            _tree = tree;
            _gstate = new CSharpGlobalState();

            _rstate = new RewindableState
            {
                Output = new StringBuilder(),
                TokenIndex = 0
            };
        }

        public string GenerateCSharp()
        {
            Visit(_tree);
            return CSharpFormatter.Format(
                csharpCode: _rstate.Output.ToString(),
                state: _gstate,
                options: _options);
        }

        #region Private helper methods

        private void MovePast(ITerminalNode currentNode)
        {
            D.AssertNotNull(currentNode);

            ProcessHiddenTokensBefore(currentNode.Symbol);
            _rstate.TokenIndex++;
        }

        private void MovePast(ParserRuleContext currentContext)
        {
            D.AssertNotNull(currentContext);

            ProcessHiddenTokensBefore(currentContext.Start);
            _rstate.TokenIndex += currentContext.DescendantTokenCount();
        }

        private void MovePast(IParseTree currentTree)
        {
            D.AssertNotNull(currentTree);

            switch (currentTree)
            {
                case ITerminalNode currentNode:
                    MovePast(currentNode);
                    break;
                case ParserRuleContext currentContext:
                    MovePast(currentContext);
                    break;
                default:
                    D.Fail($"Unrecognized {nameof(IParseTree)} subclass: {currentTree.GetType()}");
                    break;
            }
        }

        private void ProcessHiddenToken(IToken hiddenToken)
        {
            D.AssertNotNull(hiddenToken);

            Write(hiddenToken.Text);
            _rstate.TokenIndex++;
        }

        private void ProcessHiddenTokensBefore(IToken currentToken)
        {
            D.AssertNotNull(currentToken);

            int start = _rstate.TokenIndex;
            int end = currentToken.TokenIndex;

            for (int i = start; i < end; i++)
            {
                ProcessHiddenToken(_tokenStream.Get(i));
            }
        }

        private string Record(Action action)
        {
            var tempState = RunAndRewind(action);
            string result = tempState.Output.ToString();
            tempState.Output = _rstate.Output;
            _rstate = tempState;
            return result;
        }

        /*
        private string RecordAndRewind(Action action)
        {
            var tempState = RunAndRewind(action);
            return tempState.Output.ToString();
        }
        */

        private RewindableState RunAndRewind(Action action)
        {
            var originalState = _rstate.Clone();
            _rstate.ResetOutput();
            action();

            var tempState = _rstate;
            _rstate = originalState;
            return tempState;
        }

        private void Write(string csharpText)
        {
            _rstate.Output.Append(csharpText);
        }

        #endregion

        #region Visit methods

        public override Unit VisitErrorNode(IErrorNode node)
        {
            MovePast(node);
            Write(node.GetText());
            return default;
        }

        public override Unit VisitTerminal([NotNull] ITerminalNode node)
        {
            MovePast(node);
            // Reference: https://docs.oracle.com/javase/tutorial/java/nutsandbolts/_keywords.html
            switch (node.Symbol.Type)
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
                case INSTANCEOF:
                    Write("is");
                    break;
                case NATIVE:
                    Write("extern");
                    break;
                case SUPER:
                    Write("base");
                    break;
                case SYNCHRONIZED:
                    Write("lock");
                    break;
                case TRANSIENT:
                    // Don't write anything.
                    break;
                default:
                    Write(node.GetText());
                    break;
            }

            return default;
        }

        #region Annotations

        public override Unit VisitAnnotation([NotNull] AnnotationContext context)
        {
            // Omit the annotations entirely from the C# output.
            MovePast(context);
            return default;
        }

        #endregion

        #region Assertions

        public override Unit VisitAssertStatementNoMessage([NotNull] AssertStatementNoMessageContext context)
        {
            // 'assert' expression ';'
            var expression = context.expression();

            _gstate.AddUsing("System.Diagnostics");

            MovePast(context.GetChild(0));
            Write("Debug.Assert(");
            Visit(expression);
            MovePast(context.GetChild(2));
            Write(");");
            return default;
        }

        public override Unit VisitAssertStatementWithMessage([NotNull] AssertStatementWithMessageContext context)
        {
            // 'assert' expression ':' expression ';'
            var expression1 = (ExpressionContext)context.GetChild(1);
            var expression2 = (ExpressionContext)context.GetChild(3);

            _gstate.AddUsing("System.Diagnostics");

            MovePast(context.GetChild(0));
            Write("Debug.Assert(");
            Visit(expression1);
            MovePast(context.GetChild(2));
            Write(", ");
            Visit(expression2);
            MovePast(context.GetChild(4));
            Write(");");
            return default;
        }

        #endregion

        #region Class declarations

        public override Unit VisitClassInstanceCreationExpression([NotNull] ClassInstanceCreationExpressionContext context)
        {
            return CommonVisitClassInstanceCreationExpression(context);
        }

        public override Unit VisitClassInstanceCreationExpression_lf_primary([NotNull] ClassInstanceCreationExpression_lf_primaryContext context)
        {
            return CommonVisitClassInstanceCreationExpression(context);
        }

        public override Unit VisitClassInstanceCreationExpression_lfno_primary([NotNull] ClassInstanceCreationExpression_lfno_primaryContext context)
        {
            return CommonVisitClassInstanceCreationExpression(context);
        }

        private Unit CommonVisitClassInstanceCreationExpression(ParserRuleContext context)
        {
            var identifierNode = context.GetFirstToken(Identifier);
            var classBodyOrNot = context.GetFirstChild<AnonymousClassBodyOrNotContext>();

            this.VisitChildrenBefore(identifierNode, context);

            string anonymousClassBody = default;
            RunAndRewind(() =>
            {
                Visit(identifierNode);
                this.VisitChildrenBetween(identifierNode, classBodyOrNot, context);
                anonymousClassBody = Record(() => Visit(classBodyOrNot));
            });

            if (!string.IsNullOrEmpty(anonymousClassBody))
            {
                string className = _gstate.AddAnonymousClass(anonymousClassBody);
                MovePast(identifierNode);
                Write(className);
            }
            else
            {
                Visit(identifierNode);
            }

            this.VisitChildrenBetween(identifierNode, classBodyOrNot, context);
            MovePast(classBodyOrNot);
            return default;
        }

        #endregion

        #region Import declarations

        public override Unit VisitSingleTypeImportDeclaration([NotNull] SingleTypeImportDeclarationContext context)
        {
            // 'import' typeName ';'
            string typeName = context.typeName().GetText();

            string packageName = GetPackageName(typeName);
            string namespaceName = ConvertPackageName(packageName);
            _gstate.AddUsing(namespaceName);

            MovePast(context);
            return default;
        }

        public override Unit VisitTypeImportOnDemandDeclaration([NotNull] TypeImportOnDemandDeclarationContext context)
        {
            // 'import' packageOrTypeName '.' '*' ';'
            // We can't detect whether the packageOrTypeName node refers to a package or a type, so just assume it's referring to a package.
            string packageName = context.packageOrTypeName().GetText();

            string namespaceName = ConvertPackageName(packageName);
            _gstate.AddUsing(namespaceName);

            MovePast(context);
            return default;
        }

        #endregion

        #region Local variables

        public override Unit VisitLocalVariableFinalModifier([NotNull] LocalVariableFinalModifierContext context)
        {
            // localVariableFinalModifier : 'final'
            // 'final Foo local' => 'Foo local' (just erase the 'final', as C# does not yet have readonly locals)
            MovePast(context);
            return default;
        }

        #endregion

        #region Method declarations

        public override Unit VisitGenericMethodHeader([NotNull] GenericMethodHeaderContext context)
        {
            // typeParameters methodAnnotations result methodDeclarator throws_OrNot
            var typeParametersNode = context.typeParameters();
            var methodAnnotations = context.methodAnnotations();


            string typeParameters = default;
            RunAndRewind(() =>
            {
                typeParameters = Record(() => Visit(typeParametersNode));
                Visit(methodAnnotations);
            });

            bool hasOverrideAnnotation = _methodDeclarationChannel.Receive() || _methodAnnotationChannel.ReceiveOrDefault(false);

            _genericMethodHeaderChannel.Send(typeParameters);
            if (hasOverrideAnnotation)
            {
                Write(" override ");
            }

            MovePast(typeParametersNode);
            MovePast(methodAnnotations);
            this.VisitChildrenAfter(methodAnnotations, context);
            return default;
        }

        public override Unit VisitMethodAnnotation([NotNull] MethodAnnotationContext context)
        {
            _methodAnnotationChannel.Send(context.GetText() == "@Override");
            return base.VisitMethodAnnotation(context);
        }

        public override Unit VisitMethodDeclaration([NotNull] MethodDeclarationContext context)
        {
            // methodModifiers methodHeader methodBody
            var methodModifiers = context.methodModifiers();

            Visit(methodModifiers);

            bool hasOverrideAnnotation = _methodAnnotationChannel.ReceiveOrDefault(false);
            _methodDeclarationChannel.Send(hasOverrideAnnotation);

            this.VisitChildrenAfter(methodModifiers, context);
            return default;
        }

        public override Unit VisitMethodDeclarator([NotNull] MethodDeclaratorContext context)
        {
            // Identifier '(' formalParameterList? ')' dims?
            var identifierNode = context.GetFirstToken(Identifier);
            string typeParameters = _genericMethodHeaderChannel.ReceiveOrDefault(string.Empty);

            Visit(identifierNode);
            Write(typeParameters);
            this.VisitChildrenAfter(identifierNode, context);
            return default;
        }

        public override Unit VisitNonGenericMethodHeader([NotNull] NonGenericMethodHeaderContext context)
        {
            // result methodDeclarator throws_OrNot
            bool hasOverrideAnnotation = _methodDeclarationChannel.Receive();

            if (hasOverrideAnnotation)
            {
                Write(" override ");
            }

            return base.VisitNonGenericMethodHeader(context);
        }

        public override Unit VisitParameterFinalModifier([NotNull] ParameterFinalModifierContext context)
        {
            // parameterFinalModifier : 'final'
            // 'final Foo parameter' => 'in Foo parameter' for C# >= 7.2, 'Foo parameter' otherwise
            MovePast(context);

            if (_options.CSharpLanguageVersion >= LanguageVersion.CSharp7_2)
            {
                Write(" in ");
            }

            return default;
        }

        public override Unit VisitThrows_OrNot([NotNull] Throws_OrNotContext context)
        {
            // throws_OrNot : throws_?
            // Exclude checked exceptions from the C# output.
            MovePast(context);
            return default;
        }

        #endregion

        #region Method references

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
            var methodNameNode = context.GetFirstChild<MethodNameContext>();
            string methodName = methodNameNode.GetText();
            var lparen = context.GetFirstToken(LPAREN);
            var argumentList = context.GetFirstChild<ArgumentListOrNotContext>().argumentList();
            var rparen = context.GetFirstToken(RPAREN);

            if (WriteGetterProperty(methodName, argumentList, null, methodNameNode, lparen, rparen) ||
                WriteSetterProperty(methodName, argumentList, null, methodNameNode, lparen, rparen))
            {
                return default;
            }

            string csharpMethodName = ConvertMethodName(methodName);

            MovePast(methodNameNode);
            Write(csharpMethodName);
            this.VisitChildrenAfter(methodNameNode, context);
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
            var typeArgumentsOrNot = context.GetFirstChild<TypeArgumentsOrNotContext>();
            var typeArgumentsNode = typeArgumentsOrNot.typeArguments();
            var identifierNode = context.GetFirstToken(Identifier);
            string methodName = identifierNode.GetText();
            var lparen = context.GetFirstToken(LPAREN);
            var argumentList = context.GetFirstChild<ArgumentListOrNotContext>().argumentList();
            var rparen = context.GetFirstToken(RPAREN);

            this.VisitChildrenBefore(typeArgumentsOrNot, context);

            if (WriteGetterProperty(methodName, argumentList, typeArgumentsNode, identifierNode, lparen, rparen) ||
                WriteSetterProperty(methodName, argumentList, typeArgumentsNode, identifierNode, lparen, rparen))
            {
                return default;
            }

            string csharpMethodName = ConvertMethodName(methodName);

            string typeArguments = Record(() => Visit(typeArgumentsOrNot));
            MovePast(identifierNode);
            Write(csharpMethodName);
            Write(typeArguments);
            this.VisitChildrenAfter(identifierNode, context);
            return default;
        }

        private bool WriteGetterProperty(
            string javaMethodName,
            ArgumentListContext argumentList,
            TypeArgumentsContext typeArguments,
            IParseTree methodNameNode,
            ITerminalNode lparen,
            ITerminalNode rparen)
        {
            if (!ConvertGetterInvocation(javaMethodName, argumentList, typeArguments, out string csharpPropertyName))
            {
                return false;
            }

            MovePast(methodNameNode);
            Write(csharpPropertyName);
            MovePast(lparen);
            MovePast(rparen);
            return true;
        }

        private bool WriteSetterProperty(
            string javaMethodName,
            ArgumentListContext argumentList,
            TypeArgumentsContext typeArguments,
            IParseTree methodNameNode,
            ITerminalNode lparen,
            ITerminalNode rparen)
        {
            if (!ConvertSetterInvocation(javaMethodName, argumentList, typeArguments, out string csharpPropertyName))
            {
                return false;
            }

            MovePast(methodNameNode);
            Write(csharpPropertyName);
            MovePast(lparen);
            Write("=");
            Visit(argumentList);
            MovePast(rparen);
            return true;
        }

        #endregion

        #region Package declarations

        public override Unit VisitPackageDeclaration([NotNull] PackageDeclarationContext context)
        {
            // packageModifier* 'package' packageName ';'
            string packageName = context.packageName().GetText();

            string namespaceName = ConvertPackageName(packageName);
            _gstate.SetNamespace(namespaceName);

            MovePast(context);
            return default;
        }

        #endregion

        #region Type references

        public override Unit VisitUnannClassOrInterfaceType([NotNull] UnannClassOrInterfaceTypeContext context)
        {
            string typeName = context.GetText();
            string csharpTypeName;

            switch (typeName)
            {
                case "Object":
                    csharpTypeName = "object";
                    break;
                case "String":
                    csharpTypeName = "string";
                    break;
                default:
                    return base.VisitUnannClassOrInterfaceType(context);
            }

            MovePast(context);
            Write(csharpTypeName);
            return default;
        }

        #endregion

        #endregion
    }
}