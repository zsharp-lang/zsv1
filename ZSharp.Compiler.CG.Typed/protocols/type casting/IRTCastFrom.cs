namespace ZSharp.Compiler
{
    public interface IRTCastFrom
    {
        public Result<CastResult> Cast(Compiler compiler, CompilerObject value);
    }
}
