namespace ZSharp.Compiler
{
    public interface IRTCastTo
    {
        public IResult Cast(CompilerObject value, CompilerObject targetType);
    }
}
