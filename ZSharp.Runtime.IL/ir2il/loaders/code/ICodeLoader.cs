namespace ZSharp.Runtime.NET.IR2IL
{
    public interface ICodeLoader
    {
        public IL.Emit.ILGenerator Output { get; }

        public void Push(Type type);

        public void Pop();

        public void Pop(Type type);

        public void Pop(params Type[] types);
    }
}
