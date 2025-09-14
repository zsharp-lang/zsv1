namespace ZSharp.Compiler.Dispatchers.Typed
{
    partial class Dispatcher
    {
        public Result ImplicitCast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.ImplicitCast(@object, type);

            if (result.IsOk || !compiler.RuntimeDescriptor(@object, out var rtd))
                return result;

            if (result.IsError && rtd.Is<IRTImplicitCastTo>(out var castTo))
                result = castTo.ImplicitCast(compiler, @object, type);

            //if (result.IsError && rtd.Is<IRTImplicitCastFrom>(out var castFrom))
            //    result = castFrom.ImplicitCast(compiler, @object);

            return result;
        }
    }
}
