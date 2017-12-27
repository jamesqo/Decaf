using System.Collections.Generic;

namespace CoffeeMachine.Internal
{
    internal class CSharpGlobalState
    {
        private readonly Dictionary<string, string> _classes;
        private readonly HashSet<string> _usings;
        private readonly HashSet<string> _usingStatics;

        private string _namespace;

        public CSharpGlobalState()
        {
            _classes = new Dictionary<string, string>();
            _usings = new HashSet<string>();
            _usingStatics = new HashSet<string>();

            _namespace = string.Empty;
        }

        public Dictionary<string, string> Classes => _classes;
        public string Namespace => _namespace;
        public HashSet<string> Usings => _usings;
        public HashSet<string> UsingStatics => _usingStatics;

        public void AddAnonymousClass(string className, string classBody)
        {
            int suffix = 1;
            while (!AddClass($"{className}{suffix}", classBody))
            {
                suffix++;
            }
        }

        public bool AddClass(string className, string classBody)
        {
            return _classes.TryAdd(className, classBody);
        }

        public bool AddUsing(string @namespace) => _usings.Add(@namespace);

        public void SetNamespace(string @namespace) => _namespace = @namespace;
    }
}