namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult ImplicitCast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.ImplicitCast(@object, type);

            if (result.IsError && @object.Is<ICTImplicitCastTo>(out var castTo))
                result = castTo.ImplicitCast(compiler, type);

            if (result.IsError && type.Is<ICTImplicitCastFrom>(out var castFrom))
                result = castFrom.ImplicitCast(compiler, @object);

            return result;
        }
    }
}
