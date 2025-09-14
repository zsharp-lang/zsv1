namespace ZSharp.Compiler
{
    public delegate Result<CastResult> Cast(CompilerObject @object, CompilerObject type);

    partial struct CG
    {
        public Cast Cast { get; set; } = Dispatcher.Cast;
    }
}
