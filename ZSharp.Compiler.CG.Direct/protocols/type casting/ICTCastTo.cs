namespace ZSharp.Compiler
{
    public interface ICTCastTo
    {
        public Result<CastResult> Cast(Compiler compiler, CompilerObject targetType);
    }
}
