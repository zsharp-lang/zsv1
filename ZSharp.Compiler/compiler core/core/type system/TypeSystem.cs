using System.Diagnostics.CodeAnalysis;
using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed class TypeSystem
        : Feature
    {
        public StringType String { get; }

        public CompilerObject Type { get; }

        public CompilerObject Void { get; }

        public CompilerObject Null { get; }

        public CompilerObject Boolean { get; }

        public Int32Type Int32 { get; }

        public CompilerObject Float32 { get; }

        public CompilerObject Object { get; }

        internal TypeSystem(Compiler compiler)
            : base(compiler)
        {
            Type = new Objects.Type(compiler.RuntimeModule.TypeSystem.Type);

            String = new(compiler.RuntimeModule.TypeSystem.String, Type);
            Void = new RawType(compiler.RuntimeModule.TypeSystem.Void, Type);
            Null = new RawType(compiler.RuntimeModule.TypeSystem.Null, Type);
            Boolean = new RawType(compiler.RuntimeModule.TypeSystem.Boolean, Type);
            Int32 = new(compiler.RuntimeModule.TypeSystem.Int32, Type);
            Float32 = new RawType(compiler.RuntimeModule.TypeSystem.Float32, Type);
            Object = new RawType(compiler.RuntimeModule.TypeSystem.Object, Type);
        }

        public CompilerObject EvaluateType(CompilerObject @object)
            => Compiler.Evaluate(@object);

        public CompilerObject Array(CompilerObject type)
            => throw new NotImplementedException();

        public CompilerObject Pointer(CompilerObject type)
            => throw new NotImplementedException();

        public CompilerObject Reference(CompilerObject type)
            => throw new NotImplementedException();

        public bool IsTyped(CompilerObject @object)
            => @object is IDynamicallyTyped;

        public bool IsTyped(CompilerObject @object, [NotNullWhen(true)] out CompilerObject? type)
        {
            if (@object is IDynamicallyTyped typed)
                return (type = typed.GetType(Compiler)) is not null;

            return (type = null) is not null;
        }

        public bool IsTypeModifier(CompilerObject @object)
            => @object is ITypeModifier;

        public bool IsTypeModifier(CompilerObject @object, [NotNullWhen(true)] out CompilerObject? innerType)
        {
            if (@object is ITypeModifier modifier)
                return (innerType = modifier.InnerType) is not null;

            return (innerType = null) is not null;
        }
    }
}
