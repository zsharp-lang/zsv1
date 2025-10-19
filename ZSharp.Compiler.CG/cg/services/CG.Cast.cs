namespace ZSharp.Compiler
{
    public delegate IResult<CastResult, Error> Cast(CompilerObject @object, CompilerObject type);

    partial struct CG
    {
        public Cast Cast { get; set; } = Dispatcher.Cast;
    }
}
