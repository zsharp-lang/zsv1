using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class BoundField(Field field, CGObject instance)
        : CGObject
        , ICTReadable
    {
        public Field Field { get; } = field;

        public CGObject Instance { get; } = instance;

        public CGObject Type => throw new NotImplementedException();

        public Code Read(Compiler.Compiler compiler)
            => throw new NotImplementedException(); /*new([
                compiler.CompileIRCode(Instance),
                new IR.VM.GetField(Field.IR!) // TODO
            ])
            {
                MaxStackSize = 1,
                Types = [Field.Type]
            };*/
    }
}
