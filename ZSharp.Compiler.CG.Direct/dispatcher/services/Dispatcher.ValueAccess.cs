namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult Get(CompilerObject @object)
        {
            var result = @base.Get(@object);

            if (result.IsError && @object.Is<ICTGet>(out var get))
                result = get.Get(compiler);

            return result;
        }

        public IResult Set(CompilerObject @object, CompilerObject value)
        {
            var result = @base.Set(@object, value);

            if (result.IsError && @object.Is<ICTSet>(out var set))
                result = set.Set(compiler, value);

            return result;
        }
    }
}
