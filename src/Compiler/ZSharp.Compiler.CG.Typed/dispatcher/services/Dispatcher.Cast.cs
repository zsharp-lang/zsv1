namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public IResult Cast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.Cast(@object, type);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd))
                if (ts.IsAssignableTo(type, rtd))
                    result = Result.Ok(@object);
                else if (rtd.Is<IRTCastTo>(out var castTo))
                    result = castTo.Cast(@object, type);

            return result;
        }
    }
}
