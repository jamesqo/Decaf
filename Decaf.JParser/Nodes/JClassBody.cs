using System.Collections.Generic;

namespace CoffeeMachine.JParser.Nodes
{
    public class JClassBody : JNode
    {
        // Without unknown nodes: IEnumerable<JClassBodyDeclaration>
        public IEnumerable<JNode> Declarations { get; }

        public override IEnumerable<JNode> GetChildren()
        {
            foreach (var child in Declarations)
            {
                yield return child;
            }
        }
    }
}