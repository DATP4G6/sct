using System.Collections;

namespace Sct.Compiler
{
    public class StackAdapter<TBase> : IEnumerable<TBase>
    {
        private readonly Stack<StackItem<TBase>> _stack = new();

        public void Push(TBase node)
        {
            _stack.Push(new StackItem<TBase>(node));
        }

        public void PushMarker() => _stack.Push(new StackItem<TBase>());

        public TBase[] PopUntilMarker()
        {
            List<TBase> items = [];
            while (!_stack.Peek().IsMarker)
            {
                items.Add(_stack.Pop().Value!);
            }

            // pop the marker itself
            _ = _stack.Pop();

            // Popping stack reversed order of items
            items.Reverse();
            return items.ToArray();
        }

        private sealed class StackItem<T>
        {
            public T? Value { get; }
            public bool IsMarker { get; }

            public StackItem(T value)
            {
                Value = value;
                IsMarker = false;
            }

            public StackItem()
            {
                Value = default;
                IsMarker = true;
            }
        }

        public IEnumerator<TBase> GetEnumerator()
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack.ElementAt(i).IsMarker)
                {
                    continue;
                }
                yield return _stack.ElementAt(i).Value ?? throw new InvalidOperationException("Stack item is null");
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
