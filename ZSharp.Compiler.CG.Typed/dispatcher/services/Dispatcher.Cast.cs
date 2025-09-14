namespace ZSharp.Compiler.Dispatchers.Typed
{
    partial class Dispatcher
    {
        public Result<CastResult> Cast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.Cast(@object, type);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTCastTo>(out var castTo)) 
                result = castTo.Cast(compiler, @object, type);

            return result;
        }
    }
}
