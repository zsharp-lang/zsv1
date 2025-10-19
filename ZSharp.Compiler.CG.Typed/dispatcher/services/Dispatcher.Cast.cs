namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public IResult<CastResult, Error> Cast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.Cast(@object, type);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTCastTo>(out var castTo)) 
                result = castTo.Cast(compiler, @object, type);

            return result;
        }
    }
}
