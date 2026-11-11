namespace ZSharp.Compiler
{
    /// <summary>
    /// Applies an operation to candidates and gives the result of the one chosen.
    /// </summary>
    public delegate IResult Apply(CompilerObject[] candidates, Func<CompilerObject, IResult> operation);

    partial class Overloading
    {
        public Apply Apply { get; set; } = Dispatcher.Apply;
    }
}
