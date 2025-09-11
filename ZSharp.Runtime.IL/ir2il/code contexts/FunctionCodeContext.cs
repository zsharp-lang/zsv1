using CommonZ.Utils;
using ZSharp.IR;

namespace ZSharp.Runtime.NET.IR2IL.Code
{
    internal sealed class FunctionCodeContext(IRLoader loader, IL.Emit.ILGenerator il, Function function)
        : ICodeContext
        , IBranchingCodeContext
        , IFrameCodeContext
        , IFunctionalCodeContext
    {
        private readonly Mapping<IR.VM.Local, Local> locals = [];
        private readonly Mapping<IR.Parameter, Parameter> parameters = [];
        private readonly Mapping<IR.VM.Instruction, IL.Emit.Label> labels = [];

        public IL.Emit.ILGenerator IL { get; } = il;

        public CodeStack Stack { get; } = new();

        public IRLoader Loader { get; } = loader;

        public Function Function { get; } = function;

        void IBranchingCodeContext.AddBranchTarget(IR.VM.Instruction target)
            => labels[target] = IL.DefineLabel();

        IL.Emit.Label IBranchingCodeContext.GetBranchTarget(IR.VM.Instruction target)
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
                    Type = Loader.LoadType(parameter.Type)
                };

            if (Function.HasBody && Function.Body.HasLocals)
                foreach (var local in Function.Body.Locals)
                    locals[local] = new()
                    {
                        Index = local.Index,
                        Name = local.Name,
                        Type = Loader.LoadType(local.Type)
                    };
        }

        public static FunctionCodeContext From(IRLoader loader, IL.Emit.ConstructorBuilder constructor, Function function)
        {
            var context = new FunctionCodeContext(loader, constructor.GetILGenerator(), function);

            context.SetupFromIR();

            return context;
        }

        public static FunctionCodeContext From(IRLoader loader, IL.Emit.MethodBuilder method, Function function)
        {
            var context = new FunctionCodeContext(loader, method.GetILGenerator(), function);

            context.SetupFromIR();

            return context;
        }
    }
}
