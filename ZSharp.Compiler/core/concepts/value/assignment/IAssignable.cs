namespace ZSharp.Compiler
{
    public interface ICTAssignable : ICTSet
    {
        CompilerObjectResult ICTSet.Set(Compiler compiler, CompilerObject value)
            => CompilerObjectResult.Ok(Assign(compiler, value));

        public CompilerObject Assign(Compiler compiler, CompilerObject value);
    }

    public interface IRTAssignable : IRTSet
    {
        CompilerObjectResult IRTSet.Set(Compiler compiler, CompilerObject @object, CompilerObject value)
            => CompilerObjectResult.Ok(Assign(compiler, @object, value));

        public CompilerObject Assign(Compiler compiler, CompilerObject @object, CompilerObject value);
    }
}
