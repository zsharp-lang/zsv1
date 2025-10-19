namespace ZSharp.Compiler
{
    public interface IScopeContext
        : IContext
        , ILookupContext
    {
        public IResult Add(MemberName name, CompilerObject value);

        public IResult Set(MemberName name, CompilerObject value);
    }
}
