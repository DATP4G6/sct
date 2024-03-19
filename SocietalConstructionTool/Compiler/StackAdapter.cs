using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public class StackAdapter<TBase>
    {
        private readonly Stack<TBase> _stack = new();

        public void Push(TBase node)
        {
            _stack.Push(node);
        }
        public TBase Pop() => Pop<TBase>();
        public T Pop<T>() where T : TBase
        {
            var node = _stack.Pop();
            if (node is T t)
            {
                return t;
            }
            throw new UnrecognizedNodeException(nameof(T), (node?.GetType().Name) ?? "null");
        }
        public TBase Peek() => Peek<TBase>();
        public T Peek<T>() where T : TBase
        {
            var node = _stack.Peek();
            if (node is T t)
            {
                return t;
            }
            throw new UnrecognizedNodeException(nameof(T), (node?.GetType().Name) ?? "null");
        }
        public TBase[] ToArray() => ToArray<TBase>();
        public T[] ToArray<T>() where T : TBase
        {
            var arr = _stack.ToArray();
            var typedArr = _stack.OfType<T>().ToArray();
            if (arr.Length != typedArr.Length)
            {
                throw new UnrecognizedNodeException("Not all children were of type " + nameof(T));
            }
            return typedArr;
        }

        public TItem[] PopUntil<TParent, TItem>(out TParent parent)
            where TParent : TBase
            where TItem : TBase
        {
            List<TItem> items = [];
            while (_stack.Peek() is TItem item and not TParent)
            {
                items.Add(item);
                _ = _stack.Pop();
            }

            var node = _stack.Pop();
            if (node is TParent parentNode)
            {
                parent = parentNode;
            }
            else
            {
                throw new UnrecognizedNodeException(nameof(TParent), (node?.GetType().Name) ?? "null");
            }

            // Popping stack reversed order of items
            items.Reverse();
            return items.ToArray();
        }
    }
}
