namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A value seen as one of its base types, so that operations on it are those of
    /// that type. It compiles to the value itself.
    /// </summary>
    internal sealed class Upcast(CompilerObject value, CompilerObject type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Core.Compiler.ModuleScope.IR.CompileExpression(value, target);
    }
}
