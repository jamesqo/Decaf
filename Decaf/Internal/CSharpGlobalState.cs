using System.Collections.Generic;

namespace CoffeeMachine.Internal
{
    internal class CSharpGlobalState
    {
        private readonly Dictionary<string, CSharpClassInfo> _classes;
        private readonly HashSet<string> _usings;
        private readonly HashSet<string> _usingStatics;

        private string _namespace;

        public CSharpGlobalState()
        {
            _classes = new Dictionary<string, CSharpClassInfo>();
            _usings = new HashSet<string>();
            _usingStatics = new HashSet<string>();

            _namespace = string.Empty;
        }

        public Dictionary<string, CSharpClassInfo> Classes => _classes;
        public string Namespace => _namespace;
        public HashSet<string> Usings => _usings;
        public HashSet<string> UsingStatics => _usingStatics;

        public string AddAnonymousClass(string name, CSharpClassInfo info)
        {
            for (int suffix = 1; ; suffix++)
            {
                string fullName = $"{name}{suffix}";
                if (AddClass(fullName, info))
                {
                    return fullName;
                }
            }
        }

        public bool AddClass(string name, CSharpClassInfo info)
        {
            return _classes.TryAdd(name, info);
        }

        public bool AddUsing(string @namespace) => _usings.Add(@namespace);

        public void SetNamespace(string @namespace) => _namespace = @namespace;
    }
}