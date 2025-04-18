﻿using System.Diagnostics.CodeAnalysis;
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

        internal TypeSystem(Compiler compiler)
            : base(compiler)
        {
            Type = new Objects.Type(compiler.RuntimeModule.TypeSystem.Type);

            String = new(compiler.RuntimeModule.TypeSystem.String, Type);
            Void = new RawType(compiler.RuntimeModule.TypeSystem.Void, Type);
            Null = new RawType(compiler.RuntimeModule.TypeSystem.Null, Type);
            Boolean = new RawType(compiler.RuntimeModule.TypeSystem.Boolean, Type);
            Int32 = new Int32Type(compiler.RuntimeModule.TypeSystem.Int32, Type);
            Float32 = new RawType(compiler.RuntimeModule.TypeSystem.Float32, Type);
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
        {
            throw new NotImplementedException();
        }

        public bool IsTyped(CompilerObject @object, [NotNullWhen(true)] out CompilerObject? type)
        {
            if (@object is ITyped typed)
                return (type = typed.GetType(Compiler)) is not null;

            return (type = null) is not null;
        }

        public bool IsTypeModifier(CompilerObject @object)
        {
            throw new NotImplementedException();
        }

        public bool IsTypeModifier(CompilerObject @object, [NotNullWhen(true)] out CompilerObject? type)
        {
            throw new NotImplementedException();
        }
    }
}
