namespace ZSharp.Runtime.NET.IR2IL.Code
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
    }
}
