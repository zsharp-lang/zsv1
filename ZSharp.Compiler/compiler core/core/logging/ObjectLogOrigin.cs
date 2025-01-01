namespace ZSharp.Compiler
{
    internal sealed class ObjectLogOrigin(CompilerObject @object) : LogOrigin
    {
        public override string ToString()
            => @object.ToString() ?? $"[{@object.GetType()} object]";
    }
}
