using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Platform.Runtime.Loaders
{
    internal sealed class UnboundCodeContext(Runtime runtime, Emit.ILGenerator il, IContext? inner = null)
        : ICodeContext
        , IBranchingCodeContext
    {
        private readonly IContext? inner = inner;
        private readonly Mapping<IR.VM.Instruction, Emit.Label> labels = [];

        public Emit.ILGenerator IL { get; } = il;

        public CodeStack Stack { get; } = new();

        public Runtime Runtime { get; } = runtime;

        void IBranchingCodeContext.AddBranchTarget(IR.VM.Instruction target)
            => labels[target] = IL.DefineLabel();

        Emit.Label IBranchingCodeContext.GetBranchTarget(IR.VM.Instruction target)
            => labels[target];

        bool IContext.Is<T>([NotNullWhen(true)] out T? context) where T : class
            => (context = this as T) is not null || (inner?.Is(out context) ?? false);
    }
}
