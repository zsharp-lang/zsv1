namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult Get(CompilerObject @object)
        {
            var result = @base.Get(@object);

            if (result.IsUnsupported() && @object.Is<ICTGet>(out var get))
                result = get.Get();

            return result;
        }

        public IResult Set(CompilerObject @object, CompilerObject value)
        {
            var result = @base.Set(@object, value);

            if (result.IsUnsupported() && @object.Is<ICTSet>(out var set))
                result = set.Set(value);

            return result;
        }
    }
}
