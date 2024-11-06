namespace ZSharp.Compiler
{
    public interface ICTAssignable
    {
        public CGObject Assign(Compiler compiler, CGObject value);
    }

    public interface IRTAssignable
    {
        public CGObject Assign(Compiler compiler, CGObject @object, CGObject value);
    }
}
