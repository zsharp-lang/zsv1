using CommonZ.Utils;
using ZSharp.IR;

namespace ZSharp.Runtime.Loaders
{
    internal sealed class FunctionCodeContext
        : ICodeContext
        , IBranchingCodeContext
        , IFrameCodeContext
        , IFunctionalCodeContext
    {
        private readonly Mapping<IR.VM.Local, Local> locals = [];
        private readonly Mapping<IR.Parameter, Parameter> parameters = [];
        private readonly Mapping<IR.VM.Instruction, Emit.Label> labels = [];

        public Emit.ILGenerator IL { get; }

        public CodeStack Stack { get; } = new();

        public Runtime Runtime { get; }

        public Function Function { get; }

        private FunctionCodeContext(Runtime runtime, Emit.ILGenerator il, Function function)
        {
            Runtime = runtime;
            IL = il;
            Function = function;
        }

        void IBranchingCodeContext.AddBranchTarget(IR.VM.Instruction target)
            => labels[target] = IL.DefineLabel();

        Emit.Label IBranchingCodeContext.GetBranchTarget(IR.VM.Instruction target)
            => labels[target];

        Local IFrameCodeContext.GetLocal(IR.VM.Local local)
            => locals[local];

        Parameter IFrameCodeContext.GetParameter(IR.Parameter parameter)
            => parameters[parameter];

        private void SetupFromIR()
        {
            foreach (var parameter in Function.Signature.GetParameters())
                parameters[parameter] = new()
                {
                    Index = parameter.Index,
                    Name = parameter.Name,
                    Type = Runtime.ImportType(parameter.Type)
                };

            if (Function.HasBody && Function.Body.HasLocals)
                foreach (var local in Function.Body.Locals)
                {
                    var il = locals[local] = new()
                    {
                        Index = local.Index,
                        Name = local.Name,
                        Type = Runtime.ImportType(local.Type)
                    };

                    IL.DeclareLocal(il.Type);
                }
        }

        public static FunctionCodeContext From(Runtime runtime, Emit.ConstructorBuilder constructor, Function function)
            => From(runtime, constructor.GetILGenerator(), function);

        public static FunctionCodeContext From(Runtime runtime, Emit.MethodBuilder method, Function function)
            => From(runtime, method.GetILGenerator(), function);

        public static FunctionCodeContext From(Runtime runtime, Emit.ILGenerator il, Function function)
        {
            var context = new FunctionCodeContext(runtime, il, function);

            context.SetupFromIR();

            return context;
        }
    }
}
