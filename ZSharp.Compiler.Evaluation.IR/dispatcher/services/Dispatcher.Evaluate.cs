namespace ZSharp.Compiler.EvaluatorDispatchers.IR
{
    partial class Dispatcher
    {
        public IResult Evaluate(CompilerObject @object)
        {
            var result = @base.Evaluate(@object);

            if (result.IsOk) return result;

            var irCodeResult = compiler.IR.CompileCode(@object, runtime);

            if (irCodeResult.When(out var irCode).Error(out var error))
                return Result.Error(error);

            var value = runtime.Evaluate(irCode!.Instructions, irCode.RequireValueType());

            return loader.Load(value);
        }
    }
}
