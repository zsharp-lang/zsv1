namespace ZSharp.Compiler
{
    public interface ICTAssignable
    {
        public CompilerObject Assign(Compiler compiler, CompilerObject value);
    }

    public interface IRTAssignable
    {
        public CompilerObject Assign(Compiler compiler, CompilerObject @object, CompilerObject value);
    }
}
