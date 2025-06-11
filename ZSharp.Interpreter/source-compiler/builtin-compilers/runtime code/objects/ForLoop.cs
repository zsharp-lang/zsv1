using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class ForLoop
        : CompilerObject
        , ICompileIRCode
    {
        public CompilerObject Iterator { get; set; }

        public CompilerObject MoveNext { get; set; }

        public CompilerObject Current { get; set; }

        public CompilerObject Value { get; set; }

        public CompilerObject Source { get; set; }

        public IR.VM.Instruction NextLabel { get; } = new IR.VM.Nop();

        public CompilerObject For { get; set; }

        public IR.VM.Instruction ElseLabel { get; } = new IR.VM.Nop();

        public CompilerObject? Else { get; set; }

        public IR.VM.Instruction EndLabel { get; } = new IR.VM.Nop();

        public IRCode CompileIRCode(Compiler.Compiler compiler)
        {
            return new([
                .. compiler.CompileIRCode(
                    Iterator
                ).Instructions,

                NextLabel,

                .. compiler.CompileIRCode(
                    MoveNext
                ).Instructions,

                new IR.VM.JumpIfFalse(ElseLabel),

                .. compiler.CompileIRCode(
                    compiler.Cast(
                        Value,
                        compiler.TypeSystem.Void
                    )
                ).Instructions,

                .. compiler.CompileIRCode(For).Instructions,

                new IR.VM.Jump(NextLabel),

                ElseLabel,
                .. Else is null ? [] : compiler.CompileIRCode(Else).Instructions,

                EndLabel,

                new IR.VM.Pop() // Iterator
                ]
            );
        }
    }
}
