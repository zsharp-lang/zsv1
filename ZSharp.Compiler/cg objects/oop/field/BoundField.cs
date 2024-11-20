using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class BoundField(Field field, CGObject instance)
        : CGObject
        , ICTAssignable
        , ICTReadable
    {
        public Field Field { get; } = field;

        public CGObject Instance { get; } = instance;

        public CGObject? Type => Field.Type;

        public CGObject Assign(Compiler.Compiler compiler, CGObject value)
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

        public Code Read(Compiler.Compiler compiler)
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
