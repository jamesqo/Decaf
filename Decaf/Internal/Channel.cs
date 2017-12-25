using CoffeeMachine.Internal.Diagnostics;

namespace CoffeeMachine.Internal
{
    internal class Channel<T>
    {
        private T _item;
        private bool _isEmpty;

        public Channel()
        {
            _isEmpty = true;
        }

        public bool IsEmpty => _isEmpty;

        public T Receive()
        {
            D.AssertTrue(!_isEmpty, "The channel is empty.");

            T item = _item;
            _item = default;
            _isEmpty = true;
            return item;
        }

        public void Send(T item)
        {
            D.AssertTrue(_isEmpty, "Channels can only hold one item at a time.");

            _item = item;
            _isEmpty = false;
        }
    }
}
