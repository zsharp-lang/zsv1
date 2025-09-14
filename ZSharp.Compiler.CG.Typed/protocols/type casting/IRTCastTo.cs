namespace ZSharp.Compiler
{
    public interface IRTCastTo
    {
        public Result<CastResult> Cast(Compiler compiler, CompilerObject value, CompilerObject targetType);
    }
}
