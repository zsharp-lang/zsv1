using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private IResult Compile(AST.IdentifierExpression identifier)
            => Context.CurrentScope.Get(identifier.Name);
    }
}
