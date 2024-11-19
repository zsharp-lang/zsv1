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
            var code = compiler.CompileIRCode(Instance);
            var vcode = compiler.CompileIRCode(value);

            return new RawCode(new([
                ..vcode.Instructions,
                ..code.Instructions,
                new IR.VM.SetField(Field.IR!),
                ])
            {
                MaxStackSize = Math.Max(code.MaxStackSize, vcode.MaxStackSize),
                Types = [compiler.TypeSystem.Void]
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
