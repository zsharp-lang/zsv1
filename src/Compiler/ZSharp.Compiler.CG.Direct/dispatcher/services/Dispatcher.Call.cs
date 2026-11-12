namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult Call(CompilerObject callee, Argument[] arguments)
        {
            var result = @base.Call(callee, arguments);

            if (result.IsUnsupported() && callee.Is<ICTCallable>(out var callable))
                result = callable.Call(arguments);

            return result;
        }
    }
}
