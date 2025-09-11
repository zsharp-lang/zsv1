using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class BoundField(Field field, CompilerObject instance)
        : CompilerObject
        , ICTAssignable
        , ICTReadable
    {
        public Field Field { get; } = field;

        public CompilerObject Instance { get; } = instance;

        public IType? Type => Field.Type;

        public CompilerObject Assign(Compiler.Compiler compiler, CompilerObject value)
        {
            var instanceCode = compiler.CompileIRCode(Instance);
            var valueCode = compiler.CompileIRCode(value);

            return new RawCode(new([
                ..instanceCode.Instructions,
                new IR.VM.Dup(),
                ..valueCode.Instructions,
                new IR.VM.SetField(new IR.FieldReference(Field.IR!) {
                    OwningType = new IR.ClassReference(Field.IR!.Owner ?? throw new())
                }),
                new IR.VM.GetField(new IR.FieldReference(Field.IR!) {
                    OwningType = new IR.ClassReference(Field.IR!.Owner ?? throw new())
                }),
                ])
            {
                MaxStackSize = Math.Max(Math.Max(instanceCode.MaxStackSize, valueCode.MaxStackSize), 2),
                Types = [Type]
            });
        }

        public IRCode Read(Compiler.Compiler compiler)
        {
            var code = compiler.CompileIRCode(Instance);

            return new([
                ..code.Instructions,
                new IR.VM.GetField(new IR.FieldReference(Field.IR!) {
                    OwningType = new IR.ClassReference(Field.IR!.Owner ?? throw new())
                })
                ])
            {
                MaxStackSize = Math.Max(code.MaxStackSize, 1),
                Types = [Type]
            };
        }
    }
}
