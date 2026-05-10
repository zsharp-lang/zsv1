namespace ZSharp.Compiler
{
    public delegate IResult<CastResult, Error> Cast(CompilerObject @object, CompilerObject type);

    partial class CG
    {
        public Cast Cast { get; set; } = Dispatcher.Cast;
    }
}
