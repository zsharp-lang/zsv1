namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public IResult Get(CompilerObject @object)
        {
            var result = @base.Get(@object);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd) && rtd.Is<IRTGet>(out var get))
                result = get.Get(@object);

            return result;
        }

        public IResult Set(CompilerObject @object, CompilerObject value)
        {
            var result = @base.Set(@object, value);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd) && rtd.Is<IRTSet>(out var set))
                result = set.Set(@object, value);

            return result;
        }
    }
}
