using CoffeeMachine.Internal.Diagnostics;

namespace CoffeeMachine.Internal
{
    internal class Messenger<T>
    {
        private T _item;
        private bool _hasItem;

        public T Receive()
        {
            D.AssertTrue(_hasItem, "The messenger isn't carrying an item.");

            T item = _item;
            _item = default;
            _hasItem = false;
            return item;
        }

        public void Send(T item)
        {
            D.AssertTrue(!_hasItem, "Messengers can only carry one item at a time.");

            _item = item;
            _hasItem = true;
        }
    }
}