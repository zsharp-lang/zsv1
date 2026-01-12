namespace ZSharp.Compiler
{
    public interface IRTCastFrom
    {
        public IResult<CastResult, Error> Cast(Compiler compiler, CompilerObject value);
    }
}
