namespace ZSharp.Compiler
{
    public interface IImplicitCastFromValue
    {
        public CompilerObject ImplicitCastFromValue(Compiler compiler, CompilerObject value);
    }

    public interface IImplicitCastToType
    {
        public CompilerObject ImplicitCastToType(Compiler compiler, CompilerObject type);
    }
}
