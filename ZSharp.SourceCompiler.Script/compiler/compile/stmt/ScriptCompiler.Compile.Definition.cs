namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private void Compile(AST.DefinitionStatement definitionStatement)
        {
            var result = Compile(definitionStatement.Definition);

            if (result.Error(out var error))
                Interpreter.Log.Error(
                    $"Failed to compile definition: {error}",
                    new NodeLogOrigin(definitionStatement)
                );
        }
    }
}
