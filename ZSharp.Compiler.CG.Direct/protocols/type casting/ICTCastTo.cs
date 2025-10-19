namespace ZSharp.Compiler
{
    public interface ICTCastTo
    {
        public IResult<CastResult, Error> Cast(Compiler compiler, CompilerObject targetType);
    }
}
