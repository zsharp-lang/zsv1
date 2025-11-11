namespace ZSharp.SourceCompiler.Module
{
    partial class ModuleCompiler
    {
        private IResult Compile(AST.Expression expression)
            => expression switch
            {
                AST.Function function => Compile(function),
                AST.OOPDefinition oopDefinition => Compile(oopDefinition),
                _ => CompileExpression(expression)
            };
    }
}
