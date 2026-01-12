namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult Call(CompilerObject callee, Argument[] arguments)
        {
            var result = @base.Call(callee, arguments);

            if (result.IsError && callee.Is<ICTCallable>(out var callable))
                result = callable.Call(compiler, arguments);

            return result;
        }
    }
}
