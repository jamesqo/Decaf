using System;
using System.Collections.Generic;
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
    internal class DecafVisitor : Java8BaseVisitor<Unit>
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
        private RewindableState _state;

        private readonly Channel<string> _genericMethodHeaderChannel;
        private readonly Channel<bool> _methodAnnotationChannel;
        private readonly Channel<bool> _methodDeclarationChannel;
        private readonly Channel<string> _methodInvocationTypeArgumentsOrNotChannel;
        private readonly Channel<string> _methodTypeParametersChannel;

        private readonly HashSet<string> _usings;
        private readonly HashSet<string> _usingStatics;
        private string _namespace;

        public DecafVisitor(BrewOptions options, ITokenStream tokenStream, IParseTree tree)
        {
            _options = options;
            _tokenStream = tokenStream;
            _tree = tree;
            _state = new RewindableState
            {
                Output = new StringBuilder(),
                TokenIndex = 0
            };

            _genericMethodHeaderChannel = new Channel<string>();
            _methodAnnotationChannel = new Channel<bool>();
            _methodDeclarationChannel = new Channel<bool>();
            _methodInvocationTypeArgumentsOrNotChannel = new Channel<string>();
            _methodTypeParametersChannel = new Channel<string>();

            _usings = new HashSet<string>();
            _usingStatics = new HashSet<string>();
            _namespace = string.Empty;
        }

        public string GenerateCSharp()
        {
            Visit(_tree);
            return CSharpFormatter.Format(
                csharpCode: _state.Output.ToString(),
                withUsings: _usings,
                withUsingStatics: _usingStatics,
                withNamespace: _namespace,
                options: _options);
        }

        #region Private helper methods

        private bool AddUsing(string @namespace) => _usings.Add(@namespace);

        private void AdvanceTokenIndex(int offset)
        {
            _state.TokenIndex += offset;
        }

        private void ProcessHiddenToken(IToken hiddenToken)
        {
            Write(hiddenToken.Text);
        }

        private void ProcessHiddenTokensBefore(IToken currentToken)
        {
            D.AssertNotNull(currentToken);

            int start = _state.TokenIndex;
            int end = currentToken.TokenIndex;

            for (int i = start; i < end; i++)
            {
                ProcessHiddenToken(_tokenStream.Get(i));
            }
        }

        private void ProcessHiddenTokensBefore(ITerminalNode currentNode)
            => ProcessHiddenTokensBefore(currentNode.Symbol);

        private void ProcessHiddenTokensBefore(ParserRuleContext context)
            => ProcessHiddenTokensBefore(context.Start);

        private void RunAndRewind(Action action)
        {
            var originalState = _state.Clone();
            _state.ResetOutput();
            action();
            _state = originalState;
        }

        private void SetNamespace(string @namespace) => _namespace = @namespace;

        private void Write(string csharpText, IToken currentToken = null)
        {
            if (currentToken != null)
            {
                ProcessHiddenTokensBefore(currentToken);
            }

            WriteNoAdvance(csharpText);
            AdvanceTokenIndex(1);
        }

        private void Write(string csharpText, ITerminalNode currentNode)
        {
            D.AssertNotNull(currentNode);

            Write(csharpText, currentNode.Symbol);
        }

        private void WriteNoAdvance(string csharpText)
        {
            _state.Output.Append(csharpText);
        }

        #endregion

        #region Visit methods

        public override Unit VisitErrorNode(IErrorNode node)
        {
            var token = node.Symbol;
            ProcessHiddenTokensBefore(token);

            Write(token.Text);
            return default;
        }

        public override Unit VisitTerminal([NotNull] ITerminalNode node)
        {
            var token = node.Symbol;
            ProcessHiddenTokensBefore(token);

            // Reference: https://docs.oracle.com/javase/tutorial/java/nutsandbolts/_keywords.html
            switch (token.Type)
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
                    Write(token.Text);
                    break;
            }
            return default;
        }

        #region Annotations

        public override Unit VisitAnnotation([NotNull] AnnotationContext context)
        {
            ProcessHiddenTokensBefore(context);

            // Omit the annotations entirely from the C# output.
            AdvanceTokenIndex(context.DescendantTokenCount());
            return default;
        }

        #endregion

        #region Assertions

        public override Unit VisitAssertStatementNoMessage([NotNull] AssertStatementNoMessageContext context)
        {
            ProcessHiddenTokensBefore(context);

            // 'assert' expression ';'
            AddUsing("System.Diagnostics");
            Write("Debug.Assert(", (ITerminalNode)context.GetChild(0));
            Visit(context.expression());
            Write(");", (ITerminalNode)context.GetChild(2));
            return default;
        }

        public override Unit VisitAssertStatementWithMessage([NotNull] AssertStatementWithMessageContext context)
        {
            ProcessHiddenTokensBefore(context);

            // 'assert' expression ':' expression ';'
            AddUsing("System.Diagnostics");
            Write("Debug.Assert(", (ITerminalNode)context.GetChild(0));
            Visit(context.GetChild(1));
            Write(",", (ITerminalNode)context.GetChild(2));
            Visit(context.GetChild(3));
            Write(");", (ITerminalNode)context.GetChild(4));
            return default;
        }

        #endregion

        #region Import declarations

        public override Unit VisitSingleTypeImportDeclaration([NotNull] SingleTypeImportDeclarationContext context)
        {
            ProcessHiddenTokensBefore(context);

            // 'import' typeName ';'
            var typeNameNode = context.typeName();
            string typeName = typeNameNode.GetText();
            string packageName = GetPackageName(typeName);

            string csharpNamespaceName = ConvertPackageName(packageName);
            AddUsing(csharpNamespaceName);

            AdvanceTokenIndex(context.DescendantTokenCount());
            return default;
        }

        public override Unit VisitTypeImportOnDemandDeclaration([NotNull] TypeImportOnDemandDeclarationContext context)
        {
            ProcessHiddenTokensBefore(context);

            // 'import' packageOrTypeName '.' '*' ';'
            // We can't detect whether the packageOrTypeName node refers to a package or a type,
            // so just assume it's referring to a package.
            var packageNameNode = context.packageOrTypeName();
            string packageName = packageNameNode.GetText();

            string csharpNamespaceName = ConvertPackageName(packageName);
            AddUsing(csharpNamespaceName);

            AdvanceTokenIndex(context.DescendantTokenCount());
            return default;
        }

        #endregion

        #region Local variables

        public override Unit VisitLocalVariableFinalModifier([NotNull] LocalVariableFinalModifierContext context)
        {
            ProcessHiddenTokensBefore(context);

            // localVariableFinalModifier : 'final'
            // 'final Foo local' => 'Foo local' (C# does not yet have readonly locals)
            AdvanceTokenIndex(context.DescendantTokenCount());
            return default;
        }

        #endregion

        #region Method declarations

        public override Unit VisitGenericMethodHeader([NotNull] GenericMethodHeaderContext context)
        {
            ProcessHiddenTokensBefore(context);

            // methodTypeParameters methodAnnotations result methodDeclarator throws_OrNot
            var methodTypeParameters = context.methodTypeParameters();
            var methodAnnotations = context.methodAnnotations();

            RunAndRewind(() =>
            {
                Visit(methodTypeParameters);
                Visit(methodAnnotations);
            });

            string typeParameters = _methodTypeParametersChannel.Receive().Value;
            bool hasOverrideAnnotation =
                _methodDeclarationChannel.Receive().Value || _methodAnnotationChannel.Receive().GetValueOrDefault(false);

            if (hasOverrideAnnotation)
            {
                WriteNoAdvance("override ");
            }

            AdvanceTokenIndex(methodTypeParameters.DescendantTokenCount());
            ProcessHiddenTokensBefore(methodAnnotations);
            AdvanceTokenIndex(methodAnnotations.DescendantTokenCount());
            _genericMethodHeaderChannel.Send(typeParameters);
            this.VisitChildrenAfter(methodAnnotations, context);
            return default;
        }

        public override Unit VisitMethodAnnotation([NotNull] MethodAnnotationContext context)
        {
            ProcessHiddenTokensBefore(context);

            if (context.GetText() == "@Override")
            {
                _methodAnnotationChannel.Send(true);
            }

            return base.VisitMethodAnnotation(context);
        }

        public override Unit VisitMethodDeclaration([NotNull] MethodDeclarationContext context)
        {
            ProcessHiddenTokensBefore(context);

            // methodDeclaration : methodModifiers methodHeader methodBody
            var methodModifiers = context.methodModifiers();
            var methodHeader = context.methodHeader();

            Visit(methodModifiers);
            bool hasOverrideAnnotation = _methodAnnotationChannel.Receive().GetValueOrDefault(false);

            _methodDeclarationChannel.Send(hasOverrideAnnotation);
            Visit(methodHeader);

            Visit(context.GetChild(2));
            return default;
        }

        public override Unit VisitMethodDeclarator([NotNull] MethodDeclaratorContext context)
        {
            ProcessHiddenTokensBefore(context);

            var identifier = context.GetFirstToken(Identifier);
            string typeParameters = _genericMethodHeaderChannel.Receive().GetValueOrDefault(string.Empty);

            Visit(identifier);
            WriteNoAdvance(typeParameters);
            this.VisitChildrenAfter(identifier, context);
            return default;
        }

        public override Unit VisitMethodTypeParameters([NotNull] MethodTypeParametersContext context)
        {
            ProcessHiddenTokensBefore(context);

            Visit(context.typeParameters());
            _methodTypeParametersChannel.Send(_state.Output.ToString());
            return default;
        }

        public override Unit VisitNonGenericMethodHeader([NotNull] NonGenericMethodHeaderContext context)
        {
            ProcessHiddenTokensBefore(context);

            // result methodDeclarator throws_OrNot
            bool hasOverrideAnnotation = _methodDeclarationChannel.Receive().Value;

            if (hasOverrideAnnotation)
            {
                WriteNoAdvance("override ");
            }

            return base.VisitNonGenericMethodHeader(context);
        }

        public override Unit VisitParameterFinalModifier([NotNull] ParameterFinalModifierContext context)
        {
            ProcessHiddenTokensBefore(context);

            // parameterFinalModifier : 'final'
            // 'final Foo parameter' => 'in Foo parameter' for C# >= 7.2, 'Foo parameter' otherwise
            if (_options.CSharpLanguageVersion >= LanguageVersion.CSharp7_2)
            {
                WriteNoAdvance("in ");
            }

            AdvanceTokenIndex(context.DescendantTokenCount());
            return default;
        }

        public override Unit VisitThrows_OrNot([NotNull] Throws_OrNotContext context)
        {
            ProcessHiddenTokensBefore(context);

            // throws_OrNot : throws_?
            // Exclude checked exceptions from the C# output.
            AdvanceTokenIndex(context.DescendantTokenCount());
            return default;
        }

        #endregion

        #region Method invocations

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
            ProcessHiddenTokensBefore(context);

            // methodName '(' argumentListOrNot ')'
            // methodName : Identifier
            // argumentListOrNot : argumentList?
            var methodNameNode = context.GetFirstChild<MethodNameContext>().Identifier();
            string methodName = methodNameNode.GetText();
            var argumentListOrNot = context.GetFirstChild<ArgumentListOrNotContext>();
            var argumentList = argumentListOrNot.argumentList();

            if (WriteGetterProperty(methodName, argumentList, null, identifier, lparen, rparen) ||
                WriteSetterProperty(methodName, argumentList, null, identifier, lparen, rparen))
            {
                return default;
            }

            string csharpMethodName = ConvertMethodName(methodName);

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
            ProcessHiddenTokensBefore(context);

            var methodInvocationTypeArgumentsOrNot = context.GetFirstChild<MethodInvocationTypeArgumentsOrNotContext>();
            var typeArgumentsNode = methodInvocationTypeArgumentsOrNot.typeArgumentsOrNot().typeArguments();
            var identifier = context.GetFirstToken(Identifier);
            string methodName = identifier.GetText();
            var argumentListOrNot = context.GetFirstChild<ArgumentListOrNotContext>();
            var argumentList = argumentListOrNot.argumentList();

            this.VisitChildrenBefore(methodInvocationTypeArgumentsOrNot, context);

            if (WriteGetterProperty(methodName, argumentList, typeArgumentsNode, identifier, lparen, rparen) ||
                WriteSetterProperty(methodName, argumentList, typeArgumentsNode, identifier, lparen, rparen))
            {
                return default;
            }

            var lparen = context.GetFirstToken(LPAREN);
            var rparen = context.GetFirstToken(RPAREN);
            string csharpMethodName = ConvertMethodName(methodName);

            RunAndRewind(() => Visit(methodInvocationTypeArgumentsOrNot));
            string typeArguments = _methodInvocationTypeArgumentsOrNotChannel.Receive().Value;

            WriteNoAdvance(csharpMethodName);
            WriteNoAdvance(typeArguments);
            AdvanceTokenIndex(methodInvocationTypeArgumentsOrNot.DescendantTokenCount());
            ProcessHiddenTokensBefore(identifier);
            AdvanceTokenIndex(identifier.DescendantTokenCount());
            Visit(lparen);
            Visit(argumentListOrNot);
            Visit(rparen);
            return default;
        }

        public override Unit VisitMethodInvocationTypeArgumentsOrNot([NotNull] MethodInvocationTypeArgumentsOrNotContext context)
        {
            ProcessHiddenTokensBefore(context);

            Visit(context.typeArgumentsOrNot());
            _methodInvocationTypeArgumentsOrNotChannel.Send(_state.Output.ToString());
            return default;
        }

        private bool WriteGetterProperty(
            string javaMethodName,
            ArgumentListContext argumentList,
            TypeArgumentsContext typeArguments,
            ITerminalNode identifier,
            ITerminalNode lparen,
            ITerminalNode rparen)
        {
            if (!ConvertGetterInvocation(javaMethodName, argumentList, typeArguments, out string csharpPropertyName))
            {
                return false;
            }

            WriteNoAdvance(csharpPropertyName);
            AdvanceTokenIndex(3); // Identifier '(' ')'
            return true;
        }

        private bool WriteSetterProperty(
            string javaMethodName,
            ArgumentListContext argumentList,
            TypeArgumentsContext typeArguments,
            ITerminalNode identifier,
            ITerminalNode lparen,
            ITerminalNode rparen)
        {
            if (!ConvertSetterInvocation(javaMethodName, argumentList, typeArguments, out string csharpPropertyName))
            {
                return false;
            }

            WriteNoAdvance(csharpPropertyName);
            WriteNoAdvance("=");
            AdvanceTokenIndex(identifier.DescendantTokenCount());
            ProcessHiddenTokensBefore(lparen);
            AdvanceTokenIndex(lparen.DescendantTokenCount());
            Visit(argumentList);
            ProcessHiddenTokensBefore(rparen);
            AdvanceTokenIndex(rparen.DescendantTokenCount());
            return true;
        }

        #endregion

        #region Package declarations

        public override Unit VisitPackageDeclaration([NotNull] PackageDeclarationContext context)
        {
            ProcessHiddenTokensBefore(context);

            // packageModifier* 'package' packageName ';'
            var packageNameNode = context.packageName();
            string packageName = packageNameNode.GetText();

            string csharpNamespaceName = ConvertPackageName(packageName);
            SetNamespace(csharpNamespaceName);

            AdvanceTokenIndex(context.DescendantTokenCount());
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

            ProcessHiddenTokensBefore(context);

            WriteNoAdvance(csharpTypeName);
            AdvanceTokenIndex(context.DescendantTokenCount());
            return default;
        }

        #endregion

        #endregion
    }
}