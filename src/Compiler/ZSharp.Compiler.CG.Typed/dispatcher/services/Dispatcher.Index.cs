namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public IResult Index(CompilerObject @object, Argument[] arguments)
        {
            var result = @base.Index(@object, arguments);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd) && rtd.Is<IRTGetIndex>(out var index))
                result = index.Index(@object, arguments);

            return result;
        }

        public IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
        {
            var result = @base.Index(@object, arguments, value);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd) && rtd.Is<IRTSetIndex>(out var index))
                result = index.Index(@object, arguments, value);

            return result;
        }
    }
}
