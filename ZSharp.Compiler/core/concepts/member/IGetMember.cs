namespace ZSharp.Compiler
{
    public interface ICTGetMember_Old<M> : ICTGetMember<M>
    {
        CompilerObjectResult ICTGetMember<M>.Member(Compiler compiler, M member)
            => CompilerObjectResult.Ok(Member(compiler, member));

        public new CompilerObject Member(Compiler compiler, M member);
    }

    public interface IRTGetMember_Old<M> : IRTGetMember<M>
    {
        CompilerObjectResult IRTGetMember<M>.Member(Compiler compiler, CompilerObject @object, M member)
            => CompilerObjectResult.Ok(Member(compiler, @object, member));

        public new CompilerObject Member(Compiler compiler, CompilerObject value, M member);
    }
}
