namespace ZSharp.Compiler
{
    public interface ICTTypeCast
    {
        public CGObject Cast(ZSCompiler compiler, CGObject targetType);
    }

    public interface IRTTypeCast
    {
        public CGObject Cast(ZSCompiler compiler, CGObject @object, CGObject targetType);
    }
}
