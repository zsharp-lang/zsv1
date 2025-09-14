namespace ZSharp.Compiler.Dispatchers.Typed
{
    partial class Dispatcher
    {
        public Result Get(CompilerObject @object)
        {
            var result = @base.Get(@object);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTGet>(out var get))
                result = get.Get(compiler, @object);

            return result;
        }

        public Result Set(CompilerObject @object, CompilerObject value)
        {
            var result = @base.Set(@object, value);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTSet>(out var set))
                result = set.Set(compiler, @object, value);

            return result;
        }
    }
}
