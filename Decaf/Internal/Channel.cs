using CoffeeMachine.Internal.Diagnostics;

namespace CoffeeMachine.Internal
{
    internal struct Channel<T>
    {
        private T _value;
        private bool _hasValue;

        public bool IsEmpty => !_hasValue;

        public T Receive()
        {
            D.AssertTrue(_hasValue, "The channel is empty.");

            T value = _value;
            _value = default;
            _hasValue = false;
            return value;
        }

        public T ReceiveOrDefault(T defaultValue) => _hasValue ? Receive() : defaultValue;

        public void Send(T value)
        {
            D.AssertTrue(!_hasValue, "Channels can only hold one value at a time.");

            _value = value;
            _hasValue = true;
        }
    }
}
