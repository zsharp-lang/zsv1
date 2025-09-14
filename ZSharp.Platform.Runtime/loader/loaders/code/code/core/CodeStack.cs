namespace ZSharp.Platform.Runtime.Loaders
{
    public sealed class CodeStack
    {
        private readonly Stack<Type> ilStack = [];

        public void Dup()
            => ilStack.Push(ilStack.Peek());

        public void Put(Type type)
            => ilStack.Push(type);

        public Type Pop() => ilStack.Pop();

        public Type Pop(Type expect)
        {
            var result = Pop();
            if (result != expect) throw new();
            return result;
        }

        public Type? Top()
            => ilStack.Count == 0 ? null : ilStack.Peek();
    }
}
