namespace ZSharp.Compiler
{
    public interface ICTCastTo
        : CompilerObject
    {
        public Result<TypeCast, Error> Cast(Compiler compiler, IType targetType);
    }
}
