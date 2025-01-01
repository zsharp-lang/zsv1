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

        public CompilerObject? Type => Field.Type;

        public CompilerObject Assign(Compiler.Compiler compiler, CompilerObject value)
        {
            var instanceCode = compiler.CompileIRCode(Instance);
            var valueCode = compiler.CompileIRCode(value);

            return new RawCode(new([
                ..valueCode.Instructions,
                new IR.VM.Dup(),
                ..instanceCode.Instructions,
                new IR.VM.Swap(),
                new IR.VM.SetField(Field.IR!),
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
                new IR.VM.GetField(Field.IR!)
                ])
            {
                MaxStackSize = Math.Max(code.MaxStackSize, 1),
                Types = [Type]
            };
        }
    }
}
