namespace ZSharp.Compiler
{
    public interface ICTTypeCast
    {
        public CGObject Cast(Compiler compiler, CGObject targetType);
    }

    public interface IRTTypeCast
    {
        public CGObject Cast(Compiler compiler, CGObject @object, CGObject targetType);
    }
}
