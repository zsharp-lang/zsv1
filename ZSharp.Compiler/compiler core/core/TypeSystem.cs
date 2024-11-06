using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed class TypeSystem
    {
        private readonly Compiler compiler;

        public CGObject String { get; }

        public CGObject Type { get; }

        public CGObject Void { get; }

        public CGObject Null { get; }

        public CGObject Boolean { get; }

        public CGObject Int32 { get; }

        public CGObject Float32 { get; }

        internal TypeSystem(Compiler compiler)
        {
            this.compiler = compiler;

            String = new RawType(compiler.RuntimeModule.TypeSystem.String);
            Type = new RawType(compiler.RuntimeModule.TypeSystem.Type);
            Void = new RawType(compiler.RuntimeModule.TypeSystem.Void);
            Null = new RawType(compiler.RuntimeModule.TypeSystem.Null);
            Boolean = new RawType(compiler.RuntimeModule.TypeSystem.Boolean);
            Int32 = new RawType(compiler.RuntimeModule.TypeSystem.Int32);
            Float32 = new RawType(compiler.RuntimeModule.TypeSystem.Float32);
        }
    }
}
