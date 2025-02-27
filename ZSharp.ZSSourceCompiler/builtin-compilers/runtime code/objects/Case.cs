using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Case
        : CompilerObject
        , ICompileIRCode
    {
        public sealed class When
            : CompilerObject
        {
            public required CompilerObject Value { get; set; }

            public required CompilerObject Body { get; set; }
        }

        public required CompilerObject Value { get; set; }

        public required CompilerObject Of { get; set; }

        public List<When> Clauses { get; } = [];

        public CompilerObject? Else { get; set; }

        public IRCode CompileIRCode(Compiler.Compiler compiler)
        {
            IRCode result = new();

            var caseEnd = new IR.VM.Nop();

            var valueCode = compiler.CompileIRCode(Value);
            result.Append(valueCode);

            IRCode clauseBodies = new();

            foreach (var clause in Clauses)
            {
                var entry = new IR.VM.Pop();

                clauseBodies.Instructions.AddRange([
                    entry,
                    //new IR.VM.Pop(),
                    ..compiler.CompileIRCode(clause.Body).Instructions,
                    new IR.VM.Jump(caseEnd)
                ]);

                var condition = compiler.Call(Of, [
                    new(new RawCode(
                        new([
                            new IR.VM.Dup()
                            ]) {
                                Types = [valueCode.RequireValueType()]
                            }
                        //compiler.CompileIRCode(Value)
                    )),
                    new(clause.Value)
                ]);

                result.Instructions.AddRange([
                    ..compiler.CompileIRCode(condition).Instructions,
                    new IR.VM.JumpIfTrue(entry)
                ]);
            }

            result.Instructions.Add(new IR.VM.Pop());
            result.Types.RemoveAt(result.Types.Count - 1);

            if (Else is not null)
            {
                var entry = new IR.VM.Nop();

                var elseCode = compiler.CompileIRCode(Else);

                clauseBodies.Instructions.Add(entry);
                clauseBodies.Append(elseCode);

                result.Instructions.Add(new IR.VM.Jump(entry));
            }

            result.Append(clauseBodies);

            result.Instructions.Add(caseEnd);

            return result;
        }
    }
}
