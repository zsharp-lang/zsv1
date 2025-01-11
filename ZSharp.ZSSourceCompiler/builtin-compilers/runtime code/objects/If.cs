using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class If
        : CompilerObject
        , ICompileIRCode
    {
        public CompilerObject Body { get; set; }

        public CompilerObject Condition { get; set; }

        public IR.VM.Nop ElseLabel { get; } = new();

        public CompilerObject? Else { get; set; }

        public IR.VM.Nop EndLabel { get; } = new();

        public IRCode CompileIRCode(Compiler.Compiler compiler)
            => new([
                .. compiler.CompileIRCode(compiler.Cast(Condition, compiler.TypeSystem.Boolean)).Instructions,
                
                new IR.VM.JumpIfFalse(ElseLabel),

                .. compiler.CompileIRCode(Body).Instructions,

                new IR.VM.Jump(EndLabel),

                ElseLabel,
                .. Else is null ? [] : compiler.CompileIRCode(Else).Instructions,

                EndLabel,
                ])
            {

            };
    }
}
