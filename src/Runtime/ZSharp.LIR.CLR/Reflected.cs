using System.Reflection;

namespace ZSharp.LIR.CLR
{
    /// <summary>
    /// Describes what reflection gives as CLR handles: each type, method and field once, so
    /// the same entity is the same handle. It reads the running process, as a loader of its
    /// assemblies does, but what it gives is only data.
    /// </summary>
    public sealed class Reflected
    {
        private readonly Dictionary<System.Type, LIR.Type> types = [];
        private readonly Dictionary<MethodBase, ClrMethod> methods = [];
        private readonly Dictionary<FieldInfo, ClrField> fields = [];

        public LIR.Type Type(System.Type type)
        {
            if (types.TryGetValue(type, out var known))
                return known;

            LIR.Type described = type switch
            {
                { IsByRef: true } => new ByRefType(Type(type.GetElementType()!)),
                { IsPointer: true } => new ClrPointerType(Type(type.GetElementType()!)),
                { IsArray: true } => new ClrArrayType(Type(type.GetElementType()!), type.GetArrayRank(), type.IsSZArray),
                { IsGenericParameter: true } => new ClrGenericParameter(type.GenericParameterPosition, type.DeclaringMethod is not null),
                { IsConstructedGenericType: true } => new LIR.GenericInstance(Type(type.GetGenericTypeDefinition()), type.GetGenericArguments().Select(Type)),
                _ => new ClrType
                {
                    Assembly = type.Assembly.GetName().Name!,
                    Module = type.Module.ScopeName,
                    Token = type.MetadataToken,
                    Namespace = type.Namespace,
                    Name = type.Name,
                    DeclaringType = type.IsNested ? (ClrType)Type(type.DeclaringType!) : null,
                    IsValueType = type.IsValueType,
                }
            };

            return types[type] = described;
        }

        /// <summary>
        /// A method or constructor, with the signature its callers see: an instance method
        /// takes its object first, and a constructor gives the object it makes.
        /// </summary>
        public ClrMethod Method(MethodBase method)
        {
            if (methods.TryGetValue(method, out var known))
                return known;

            var signature = new LIR.Signature();

            if (!method.IsStatic && method is MethodInfo)
                signature.Parameters.Add(new(Type(method.DeclaringType!)) { Name = "this" });

            foreach (var parameter in method.GetParameters())
                signature.Parameters.Add(new(Type(parameter.ParameterType)) { Name = parameter.Name });

            signature.ReturnType = method switch
            {
                ConstructorInfo constructor => Type(constructor.DeclaringType!),
                MethodInfo { ReturnType: var returned } when returned != typeof(void) => Type(returned),
                _ => null
            };

            var definition = Definition(method);

            return methods[method] = new ClrMethod(signature)
            {
                Assembly = method.Module.Assembly.GetName().Name!,
                Module = method.Module.ScopeName,
                Token = method.MetadataToken,
                Name = method.Name,
                DeclaringType = Type(method.DeclaringType!),
                IsConstructor = method is ConstructorInfo,
                IsStatic = method.IsStatic,
                IsVirtual = method.IsVirtual,
                TakesAddress = !method.IsStatic && method is MethodInfo && method.DeclaringType is { IsValueType: true },
                GenericArity = definition.IsGenericMethod ? definition.GetGenericArguments().Length : 0,
                Parameters = [.. definition.GetParameters().Select(parameter => Type(parameter.ParameterType))],
                TypeArguments = method.IsGenericMethod && !method.IsGenericMethodDefinition
                    ? [.. method.GetGenericArguments().Select(Type)]
                    : [],
            };
        }

        public ClrField Field(FieldInfo field)
        {
            if (fields.TryGetValue(field, out var known))
                return known;

            return fields[field] = new ClrField
            {
                Assembly = field.Module.Assembly.GetName().Name!,
                Module = field.Module.ScopeName,
                Token = field.MetadataToken,
                Name = field.Name,
                DeclaringType = Type(field.DeclaringType!),
                IsStatic = field.IsStatic,
            };
        }

        /// <summary>
        /// The method as its definition declares it, whose parameters tell it apart from its
        /// overloads: of a generic method, its definition, and of a generic type's instance,
        /// the type's definition's.
        /// </summary>
        private static MethodBase Definition(MethodBase method)
        {
            if (method is MethodInfo { IsGenericMethod: true, IsGenericMethodDefinition: false } instance)
                method = instance.GetGenericMethodDefinition();

            if (method.DeclaringType is not { IsConstructedGenericType: true })
                return method;

            try
            {
                return method.Module.ResolveMethod(method.MetadataToken) ?? method;
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or NotSupportedException)
            {
                return method;
            }
        }
    }
}
