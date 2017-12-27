using System.Collections.Generic;

namespace CoffeeMachine.Internal
{
    internal class CSharpClassInfo
    {
        public IEnumerable<string> BaseTypes { get; set; }
        public string Body { get; set; }
        public IEnumerable<string> Modifiers { get; set; }
    }
}