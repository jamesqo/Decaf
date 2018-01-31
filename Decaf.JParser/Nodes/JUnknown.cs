using System.Collections.Generic;
using System.Text;

namespace CoffeeMachine.JParser.Nodes
{
    public class JUnknown : JNode
    {
        private readonly string _text;
        private readonly IEnumerable<JNode> _children;

        internal JUnknown(string text, IEnumerable<JNode> children)
        {
            _text = text;
            _children = children;
        }

        public override JNodeKind Kind => JNodeKind.Unknown;

        public override IEnumerable<JNode> GetChildren() => _children;

        public override void ToString(StringBuilder sb) => sb.Append(_text);
    }
}