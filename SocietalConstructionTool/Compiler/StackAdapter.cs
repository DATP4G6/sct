using System.Collections;

using Sct.Compiler.Exceptions;
using Sct.Extensions;

namespace Sct.Compiler
{
    public class StackAdapter<TBase> : IEnumerable<TBase>
    {
        private readonly Stack<StackItem<TBase>> _stack = new();

        public void Push(TBase node)
        {
            _stack.Push(new StackItem<TBase>(node));
        }
        public TBase Pop() => Pop<TBase>();
        public T Pop<T>() where T : TBase
        {
            var node = _stack.Pop();
            if (node.Value is T t)
            {
                return t;
            }
            throw new UnrecognizedNodeException(typeof(T).GenericName(), (node?.Value?.GetType().Name) ?? "null");
        }

        public TBase Peek() => Peek<TBase>();
        public T Peek<T>() where T : TBase
        {
            var node = _stack.Peek();
            if (node.Value is T t)
            {
                return t;
            }
            throw new UnrecognizedNodeException(typeof(T).GenericName(), (node?.Value?.GetType().Name) ?? "null");
        }

        public bool TryPeek(out TBase? peekedValue) => TryPeek<TBase>(out peekedValue);
        public bool TryPeek<T>(out T? peekedValue) where T : TBase
        {
            if (_stack.TryPeek(out var node) && node.Value is T value)
            {
                peekedValue = value;
                return true;
            }
            peekedValue = default;
            return false;
        }

        public TBase[] ToArray() => ToArray<TBase>();
        public T[] ToArray<T>() where T : TBase
        {
            var typedArr = _stack.Select(x => x.Value).OfType<T>().ToArray();
            if (_stack.Count != typedArr.Length)
            {
                throw new UnrecognizedNodeException("Not all children were of type " + typeof(T).GenericName());
            }
            return typedArr;
        }

        public TItem[] PopUntil<TParent, TItem>(out TParent parent)
            where TParent : TBase
            where TItem : TBase
        {
            List<TItem> items = [];
            while (_stack.Peek().Value is TItem item and not TParent)
            {
                items.Add(item);
                _ = _stack.Pop();
            }

            var node = _stack.Pop().Value;
            if (node is TParent parentNode)
            {
                parent = parentNode;
            }
            else
            {
                throw new UnrecognizedNodeException(typeof(TParent).GenericName(), (node?.GetType().Name) ?? "null");
            }

            // Popping stack reversed order of items
            items.Reverse();
            return items.ToArray();
        }

        public TItem[] PopWhile<TItem>() where TItem : TBase
        {
            List<TItem> items = [];
            while (_stack.Peek().Value is TItem item)
            {
                items.Add(item);
                _ = _stack.Pop();
            }

            // Popping stack reversed order of items
            items.Reverse();
            return items.ToArray();
        }

        public void PushMarker() => _stack.Push(new StackItem<TBase>());

        public TBase[] PopUntilMarker() => PopUntilMarker<TBase>();

        public T[] PopUntilMarker<T>() where T : TBase
        {
            List<T> items = [];
            while (_stack.Peek().Value is T item && _stack.Peek().IsMarker == false)
            {
                items.Add(item);
                _ = _stack.Pop();
            }

            var node = _stack.Pop();
            if (!node.IsMarker)
            {
                throw new UnrecognizedNodeException("Marker", (node?.Value?.GetType().Name) ?? "null");
            }

            // Popping stack reversed order of items
            items.Reverse();
            return items.ToArray();
        }

        public override string ToString()
        {
            return string.Join("\n", _stack.Select(x => x.Value?.GetType().Name ?? (x.IsMarker ? "Marker" : "null")));
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
