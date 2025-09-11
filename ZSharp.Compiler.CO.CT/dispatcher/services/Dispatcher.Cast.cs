namespace ZSharp.Compiler.Dispatchers.CT
{
    partial class Dispatcher
    {
        public Result<CastResult> Cast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.Cast(@object, type);

            if (result.IsError && @object.Is<ICTCastTo>(out var castTo)) 
                result = castTo.Cast(compiler, type);

            return result;
        }
    }
}
