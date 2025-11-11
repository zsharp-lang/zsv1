using ZSharp.IR.VM;

namespace ZSharp.Compiler.IRDispatchers.Proxy
{
    partial class Dispatcher
    {
        public IResult<IRCode, Error> CompileCode(CompilerObject @object, TargetPlatform? target)
        {
            var result = @base.CompileCode(@object, target);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => IR.CompileCode(proxied, target));

            return result;
        }
    }
}
