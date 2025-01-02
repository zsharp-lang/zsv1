using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class WhileLoop
        : CompilerObject
        , ICompileIRCode
        , ITyped
    {
        public IR.VM.Instruction ConditionLabel { get; } = new IR.VM.Nop();

        public CompilerObject Condition { get; set; }

        public CompilerObject While { get; set; }

        public IR.VM.Instruction ElseLabel { get; } = new IR.VM.Nop();

        public CompilerObject? Else { get; set; }

        public IR.VM.Instruction EndLabel { get; } = new IR.VM.Nop();

        public CompilerObject Type => throw new();

        public IRCode CompileIRCode(Compiler.Compiler compiler)
        {
            return new([
                ConditionLabel,
                .. compiler.CompileIRCode(
                    compiler.Cast(Condition, compiler.TypeSystem.Boolean)
                ).Instructions,

                new IR.VM.JumpIfFalse(ElseLabel),
                
                .. compiler.CompileIRCode(
                    compiler.Cast(While, compiler.TypeSystem.Void)
                    ).Instructions,

                new IR.VM.Jump(ConditionLabel),

                ElseLabel,
                .. Else is null ? [] : compiler.CompileIRCode(
                    compiler.Cast(Else, compiler.TypeSystem.Void)
                    ).Instructions,

                EndLabel
                ]
            );
        }

        CompilerObject IDynamicallyTyped.GetType(Compiler.Compiler compiler)
        {
            if (compiler.TypeSystem.IsTyped(While, out var type))
                return type;

            throw new();
        }
    }
}
