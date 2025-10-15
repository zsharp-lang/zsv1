namespace ZSharp.Compiler.EvaluatorDispatchers.Direct
{
    partial class Dispatcher
    {
        public Result Evaluate(CompilerObject @object)
        {
            var result = @base.Evaluate(@object);

            if (result.IsError && @object.Is<ICTEvaluate>(out var evaluate))
                result = evaluate.Evaluate(compiler);

            return result;
        }
    }
}
