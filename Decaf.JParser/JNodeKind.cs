namespace CoffeeMachine.JParser
{
    public enum JNodeKind
    {
        /// <summary>
        /// Attempts to infer the kind of node being parsed.
        /// </summary>
        Infer,

        /// <summary>
        /// Parse as a compilation unit node.
        /// </summary>
        CompilationUnit,
        /// <summary>
        /// Parse as a class body node.
        /// </summary>
        ClassBody,
        /// <summary>
        /// Parse as a method body node.
        /// </summary>
        MethodBody,
        /// <summary>
        /// Parse as an expression node.
        /// </summary>
        Expression
    }
}