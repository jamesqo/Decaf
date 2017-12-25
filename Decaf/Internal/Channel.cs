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

            T item = _value;
            _value = default;
            _hasValue = false;
            return item;
        }

        public T ReceiveOrDefault(T defaultValue) => _hasValue ? Receive() : defaultValue;

        public void Send(T item)
        {
            D.AssertTrue(!_hasValue, "Channels can only hold one item at a time.");

            _value = item;
            _hasValue = true;
        }
    }
}
