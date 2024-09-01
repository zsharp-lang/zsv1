namespace ZSharp.Compiler
{
    public interface ICTAssignable
    {
        public CTObject Assign(ZSCompiler compiler, CTObject value);
    }

    public interface IRTAssignable
    {
        public CTObject Assign(ZSCompiler compiler, CTObject @object, CTObject value);
    }
}
