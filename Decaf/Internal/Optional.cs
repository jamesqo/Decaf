using CoffeeMachine.Internal.Diagnostics;

namespace CoffeeMachine.Internal
{
    internal class Optional<T>
    {
        public static readonly Optional<T> Empty = new Optional<T>();

        private readonly T _value;
        private readonly bool _hasValue;

        private Optional()
        {
        }

        public Optional(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public bool HasValue => _hasValue;

        public T Value
        {
            get
            {
                D.AssertTrue(_hasValue, "The optional does not have an item.");

                return _value;
            }
        }

        public T GetValueOrDefault(T defaultValue) => _hasValue ? _value : defaultValue;
    }
}