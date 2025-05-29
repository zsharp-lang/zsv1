namespace ZSharp.Compiler
{
    public interface IRTCastFrom
        : CompilerObject
    {
        public Result<TypeCast, Error> Cast(Compiler compiler, CompilerObject value);
    }
}
