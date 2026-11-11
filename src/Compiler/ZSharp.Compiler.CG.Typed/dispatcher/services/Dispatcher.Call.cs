namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public IResult Call(CompilerObject callee, Argument[] arguments)
        {
            var result = @base.Call(callee, arguments);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, callee, out var rtd) && rtd.Is<IRTCallable>(out var callable))
                result = callable.Call(callee, arguments);

            return result;
        }
    }
}
