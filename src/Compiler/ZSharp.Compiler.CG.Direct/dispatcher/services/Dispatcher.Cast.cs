namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult Cast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.Cast(@object, type);

            if (result.IsUnsupported() && @object.Is<ICTCastTo>(out var castTo))
                result = castTo.Cast(type);

            if (result.IsUnsupported() && type.Is<ICTCastFrom>(out var castFrom))
                result = castFrom.Cast(@object);

            return result;
        }
    }
}
