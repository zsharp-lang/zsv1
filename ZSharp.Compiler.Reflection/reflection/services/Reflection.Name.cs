namespace ZSharp.Compiler
{
    public delegate string GetObjectName(CompilerObject @object);

    partial struct Reflection
    {
        public GetObjectName GetName { get; set; } = Dispatcher.GetName;

        public readonly bool HasName(CompilerObject @object, out string name)
            => (name = GetName(@object)) != string.Empty;
    }
}
