using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed class TypeSystem
    {
        private readonly Compiler compiler;

        public CGObject String { get; }

        public CGObject Type { get; }

        public CGObject Void { get; }

        internal TypeSystem(Compiler compiler)
        {
            this.compiler = compiler;

            String = new RawType(compiler.RuntimeModule.TypeSystem.String);
            Type = new RawType(compiler.RuntimeModule.TypeSystem.Type);
            Void = new RawType(compiler.RuntimeModule.TypeSystem.Void);
        }
    }
}
