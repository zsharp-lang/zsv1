namespace ZSharp.Compiler.Dispatchers.CT
{
    partial class Dispatcher
    {
        public Result Call(CompilerObject callee, Argument[] arguments)
        {
            var result = @base.Call(callee, arguments);

            if (result.IsError && callee.Is<ICTCallable>(out var callable))
                result = callable.Call(compiler, arguments);

            return result;
        }
    }
}
