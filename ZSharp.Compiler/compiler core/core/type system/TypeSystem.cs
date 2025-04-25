using System.Diagnostics.CodeAnalysis;
using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed class TypeSystem
        : Feature
    {
        public StringType String { get; }

        public IType Type { get; }

        public IType Void { get; }

        public IType Null { get; }

        public IType Boolean { get; }

        public Int32Type Int32 { get; }

        public IType Float32 { get; }

        public IType Object { get; }

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

        public IType EvaluateType(CompilerObject @object)
            => (IType)Compiler.Evaluate(@object);

        public IType Array(CompilerObject type)
            => throw new NotImplementedException();

        public IType Pointer(CompilerObject type)
            => throw new NotImplementedException();

        public IType Reference(CompilerObject type)
            => throw new NotImplementedException();

        public bool AreEqual(IType left, IType right)
        {
            if (left.IsEqualTo(Compiler, right))
                return true;
            if (right.IsEqualTo(Compiler, left))
                return true;
            return false;
        }

        public CompilerObjectResult ImplicitCast(CompilerObject value, IType type)
        {
            CompilerObjectResult result = CompilerObjectResult.Error(
                $"ImplicitCast for value:{value}, type:{type} is not supported."
            );

            if (type is IImplicitCastFromValue castFromValue)
                result = Compiler.Wrap(c => castFromValue.ImplicitCastFromValue(c, value));

            if (result.IsOk)
                return result;

            if (value is IImplicitCastToType castToType)
                result = Compiler.Wrap(c => castToType.ImplicitCastToType(c, type));

            if (result.IsOk)
                return result;

            if (IsTyped(value, out var valueType) && IsAssignableTo(valueType, type))
                result = CompilerObjectResult.Ok(value);

            return result;
        }

        public bool IsAssignableTo(IType source, IType target)
        {
            if (source is ITypeAssignableToType to && to.IsAssignableTo(Compiler, target) is bool assignableTo)
                return assignableTo;

            if (target is ITypeAssignableFromType from && from.IsAssignableFrom(Compiler, source) is bool assignableFrom)
                return assignableFrom;

            if (AreEqual(source, target))
                return true;

            return false;
        }

        public bool IsAssignableFrom(IType target, IType source)
            => IsAssignableTo(source, target);

        public bool IsTyped(CompilerObject @object)
            => @object is IDynamicallyTyped;

        public bool IsTyped(CompilerObject @object, [NotNullWhen(true)] out IType? type)
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
