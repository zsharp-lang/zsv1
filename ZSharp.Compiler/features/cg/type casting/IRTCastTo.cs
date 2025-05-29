namespace ZSharp.Compiler
{
    public interface IRTCastTo
        : CompilerObject
    {
        public Result<TypeCast, Error> Cast(Compiler compiler, CompilerObject value, IType targetType);
    }
}
