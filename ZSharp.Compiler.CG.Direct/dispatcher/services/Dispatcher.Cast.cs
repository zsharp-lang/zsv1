namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult<CastResult, Error> Cast(CompilerObject @object, CompilerObject type)
        {
            var result = @base.Cast(@object, type);

            if (result.IsError && @object.Is<ICTCastTo>(out var castTo)) 
                result = castTo.Cast(compiler, type);

            return result;
        }
    }
}
