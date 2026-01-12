namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private void Compile(AST.BlockStatement block)
        {
            var _ = Interpreter.Compiler.ContextScope(new ScopeContext());

            foreach (var statement in block.Statements)
                Compile(statement);
        }
    }
}
