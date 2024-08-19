namespace ZSharp.CTRuntime
{
    public interface ICTAssignable
    {
        public Code Assign(ZSCompiler compiler, Code value);
    }

    public interface IRTAssignable
    {
        public Code Assign(ZSCompiler compiler, Code @object, Code value);
    }
}
