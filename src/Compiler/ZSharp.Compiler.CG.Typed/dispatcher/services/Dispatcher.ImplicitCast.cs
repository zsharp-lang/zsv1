namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public IResult ImplicitCast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.ImplicitCast(@object, type);

            if (!result.IsUnsupported() || !RuntimeDescriptors.Find(ts, @object, out var rtd))
                return result;

            if (rtd.Is<IRTImplicitCastTo>(out var castTo))
                result = castTo.ImplicitCast(@object, type);

            if (result.IsUnsupported() && ts.IsAssignableTo(type, rtd))
                result = Result.Ok(@object);

            return result;
        }
    }
}
