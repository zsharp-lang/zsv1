using ZSharp.CGObjects;
using ZSharp.Compiler;
using ZSharp.VM;

namespace ZSharp.CT.StandardLibrary
{
    public sealed class InternalFunction
        : RTFunction
    {
        public required ZSInternalFunctionDelegate Implementation { get; init; }

        public InternalFunction(IR.Function function)
            : base(function.Name)
        {
            IR = function;
        }

        public InternalFunction(IR.IType returnType)
            : this(new IR.Function(returnType))
        {
            
        }

        public override CGObject Call(ICompiler compiler, Argument[] arguments)
        {
            var code = base.Call(compiler, arguments) as RawCode;

            code!.Code.Instructions[^1] = new IR.VM.CallInternal(IR!);

            return code;
        }
    }
}
