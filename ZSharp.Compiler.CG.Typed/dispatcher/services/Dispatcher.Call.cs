namespace ZSharp.Compiler.Dispatchers.Typed
{
    partial class Dispatcher
    {
        public Result Call(CompilerObject callee, Argument[] arguments)
        {
            var result = @base.Call(callee, arguments);

            if (result.IsError && compiler.RuntimeDescriptor(callee, out var rtd) && rtd.Is<IRTCallable>(out var callable))
                result = callable.Call(compiler, callee, arguments);

            return result;
        }
    }
}
