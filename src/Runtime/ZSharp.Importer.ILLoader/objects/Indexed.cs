namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// An element an indexer gives, as in <c>list[0]</c>: reading it calls the indexer's
    /// getter, and assigning it calls its setter with the same arguments.
    /// </summary>
    internal sealed class Indexed(CompilerObject read, Func<CompilerObject, IResult> write)
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ITyped
    {
        CompilerObject ITyped.Type => Core.Compiler.ModuleScope.TS.TypeOf(read).Unwrap();

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Core.Compiler.ModuleScope.IR.CompileExpression(read, target);

        public IResult Set(CompilerObject value)
            => write(value);
    }
}
