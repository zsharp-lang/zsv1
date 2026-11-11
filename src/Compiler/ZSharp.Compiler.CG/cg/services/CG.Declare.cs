namespace ZSharp.Compiler
{
    /// <summary>
    /// What it means for a definition to start at a point in code.
    /// </summary>
    public delegate IResult Declare(CompilerObject definition);

    partial class CG
    {
        public Declare Declare { get; set; } = Dispatcher.Declare;
    }
}
