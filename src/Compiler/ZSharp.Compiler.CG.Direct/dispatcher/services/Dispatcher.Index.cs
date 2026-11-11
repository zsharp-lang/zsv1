namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult Index(CompilerObject @object, Argument[] arguments)
        {
            var result = @base.Index(@object, arguments);

            if (result.IsUnsupported() && @object.Is<ICTGetIndex>(out var index))
                result = index.Index(arguments);

            return result;
        }

        public IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
        {
            var result = @base.Index(@object, arguments, value);

            if (result.IsUnsupported() && @object.Is<ICTSetIndex>(out var index))
                result = index.Index(arguments, value);

            return result;
        }
    }
}
