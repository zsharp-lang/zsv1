namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult Index(CompilerObject @object, Argument[] arguments)
        {
            var result = @base.GetIndex(@object, arguments);

            if (result.IsError && @object.Is<ICTGetIndex>(out var index))
                result = index.Index(compiler, arguments);

            return result;
        }

        public IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
        {
            var result = @base.GetIndex(@object, arguments);

            if (result.IsError && @object.Is<ICTSetIndex>(out var index))
                result = index.Index(compiler, arguments, value);

            return result;
        }
    }
}
