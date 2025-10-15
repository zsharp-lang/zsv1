namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private void Compile(AST.IfStatement @if)
        {
            var conditionResult = Compile(@if.Condition);

            if (
                conditionResult
                .When(out var condition)
                .Error(out var error)
            )
            {
                Interpreter.Log.Error(
                    $"Failed to compile if condition: {error}",
                    new NodeLogOrigin(@if.Condition)
                );
                return;
            }

            // TODO: cast to boolean

            var conditionValueResult = Interpreter.Evaluate(condition!);

            if (
                conditionValueResult
                .When(out var conditionValue)
                .Error(out error)
            )
            {
                Interpreter.Log.Error(
                    $"Failed to evaluate if condition: {error}",
                    new NodeLogOrigin(@if.Condition)
                );
                return;
            }

            if (conditionValue is not bool conditionBool)
            {
                Interpreter.Log.Error(
                    $"If condition does not evaluate to a boolean value.",
                    new NodeLogOrigin(@if.Condition)
                );
                return;
            }

            if (conditionBool) Compile(@if.If);
            else if (@if.Else is not null) Compile(@if.Else);
        }
    }
}
