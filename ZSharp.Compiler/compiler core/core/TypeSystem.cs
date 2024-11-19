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

            Type = new CGObjects.Type(compiler.RuntimeModule.TypeSystem.Type);

            String = new RawType(compiler.RuntimeModule.TypeSystem.String, Type);
            Void = new RawType(compiler.RuntimeModule.TypeSystem.Void, Type);
            Null = new RawType(compiler.RuntimeModule.TypeSystem.Null, Type);
            Boolean = new RawType(compiler.RuntimeModule.TypeSystem.Boolean, Type);
            Int32 = new RawType(compiler.RuntimeModule.TypeSystem.Int32, Type);
            Float32 = new RawType(compiler.RuntimeModule.TypeSystem.Float32, Type);
        }
    }
}
