namespace ZSharp.Compiler
{
    public interface IScopeContext
        : IContext
        , ILookupContext
    {
        public Result Add(MemberName name, CompilerObject value);

        public Result Set(MemberName name, CompilerObject value);
    }
}
