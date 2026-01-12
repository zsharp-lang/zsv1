namespace ZSharp.Compiler
{
    public interface IRTCastTo
    {
        public IResult<CastResult, Error> Cast(Compiler compiler, CompilerObject value, CompilerObject targetType);
    }
}
