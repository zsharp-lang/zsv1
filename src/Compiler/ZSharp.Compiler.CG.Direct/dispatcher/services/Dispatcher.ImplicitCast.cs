namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult ImplicitCast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.ImplicitCast(@object, type);

            if (result.IsUnsupported() && @object.Is<ICTImplicitCastTo>(out var castTo))
                result = castTo.ImplicitCast(type);

            if (result.IsUnsupported() && type.Is<ICTImplicitCastFrom>(out var castFrom))
                result = castFrom.ImplicitCast(@object);

            return result;
        }
    }
}
