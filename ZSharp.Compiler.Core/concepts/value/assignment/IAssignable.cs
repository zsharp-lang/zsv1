namespace ZSharp.Compiler
{
    public interface ICTAssignable
    {
        public CGObject Assign(ZSCompiler compiler, CGObject value);
    }

    public interface IRTAssignable
    {
        public CGObject Assign(ZSCompiler compiler, CGObject @object, CGObject value);
    }
}
