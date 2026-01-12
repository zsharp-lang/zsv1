namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public IResult Index(CompilerObject @object, Argument[] arguments)
        {
            var result = @base.GetIndex(@object, arguments);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTGetIndex>(out var index))
                result = index.Index(compiler, @object, arguments);

            return result;
        }

        public IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
        {
            var result = @base.GetIndex(@object, arguments);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTSetIndex>(out var index))
                result = index.Index(compiler, @object, arguments, value);

            return result;
        }
    }
}
