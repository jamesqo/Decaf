using System.Collections.Generic;

namespace CoffeeMachine.JParser.Nodes
{
    public class JClassDeclaration : JNode
    {
        public string Name { get; }
        public JClassBody Body { get; }

        public override IEnumerable<JNode> GetChildren()
        {
            yield return Body;
        }
    }
}