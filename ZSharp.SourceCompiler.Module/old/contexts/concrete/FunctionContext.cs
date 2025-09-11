using ZSharp.Compiler;
using ZSharp.Objects;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class FunctionContext(
        Compiler.Compiler compiler,
        RTFunction function
    )
        : IContext
        , IMemoryAllocator
        , IObjectContext<RTFunction>
    {
        public IContext? Parent { get; set; }

        public RTFunction Function { get; } = function;

        RTFunction IObjectContext<RTFunction>.Object => Function;

        CompilerObject IMemoryAllocator.Allocate(string name, IType type, CompilerObject? initializer)
        {
            Local local = new()
            {
                Name = name,
                Initializer = initializer,
                Type = type
            };

            local.IR = compiler.CompileIRObject<IR.VM.Local, IR.VM.FunctionBody>(local, Function.IR?.Body ?? throw new());

            return local;
        }
    }
}
