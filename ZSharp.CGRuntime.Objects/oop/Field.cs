using CommonZ.Utils;
using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class Field(string name)
        : CGObject
        , ICTReadable
    {
        public IR.Field? IR { get; set; }

        public string Name { get; set; } = name;

        public CGObject? Initializer { get; set; }

        public CGObject? Type { get; set; }

        IType ICTReadable.Type => IR!.Type;

        public Code Read(ICompiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
