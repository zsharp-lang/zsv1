namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        public CompileExpression CompileExpression { get; }

        private void Compile(AST.Statement statement)
        {
            switch (statement)
            {
                case AST.BlockStatement block: Compile(block); break;
                case AST.DefinitionStatement definition: Compile(definition); break;
                case AST.ExpressionStatement expression: Compile(expression); break;
                case AST.IfStatement @if: Compile(@if); break;
                case AST.ImportStatement import: Compile(import); break;
                default:
                    Interpreter.Log.Error(
                        $"Unsupported statement type: {statement.GetType().Name}.",
                        new NodeLogOrigin(statement)
                    );
                    break;
            }
        }
    }
}
