using System.Reflection;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Describes CLR entities as HIR. HIR holds no reflection and is never lazy, so
    /// the link back to the CLR lives here: a node is mapped to pre-created LIR
    /// handles, and the loaders registered by <see cref="Install"/> return COs
    /// primed with them. Members are discovered by those COs on demand.
    /// </summary>
    public sealed class ILLoader(Platform.Runtime.Runtime runtime)
    {
        private readonly Dictionary<Type, Package.DotNet.Class> classes = [];
        private readonly Dictionary<Package.DotNet.Class, Type> classTypes = [];
        private readonly Dictionary<Type, LIR.TypeHandle> types = [];

        private readonly Dictionary<MethodBase, Package.DotNet.Method> methods = [];
        private readonly Dictionary<Package.DotNet.Method, MethodBase> methodBases = [];
        private readonly Dictionary<MethodBase, LIR.FunctionHandle> functions = [];
        private readonly Dictionary<(FieldInfo Field, bool IsSet), LIR.FunctionHandle> fieldAccessors = [];

        public void Install(MappedLoader loader, MappedCodeLoader code)
        {
            loader.Instances.Register<Package.DotNet.Method>(LoadMethod);
            loader.Definitions.Register<Package.DotNet.Method>(LoadMethod);

            loader.Instances.Register<Package.DotNet.Class>(LoadClass);
            loader.Definitions.Register<Package.DotNet.Class>(LoadClass);
            loader.Types.Register<Package.DotNet.Class>(LoadClass);

            loader.Instances.Register<Package.DotNet.ExposedObject>(LoadExposed);

            code.Expressions.Register<HIR.Code.StringLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, typeof(string), this))
            );
            code.Expressions.Register<HIR.Code.BooleanLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, typeof(bool), this))
            );
        }

        public Package.DotNet.Class Load(Type type)
        {
            if (!classes.TryGetValue(type, out var result))
            {
                classes[type] = result = new(type.FullName ?? type.Name);
                classTypes[result] = type;
            }

            return result;
        }

        public Package.DotNet.Method Load(MethodBase method)
        {
            if (!methods.TryGetValue(method, out var result))
            {
                methods[method] = result = new(method.Name);
                methodBases[result] = method;
            }

            return result;
        }

        public Package.DotNet.ExposedObject Expose(object value)
            => new(value, Load(value.GetType()));

        internal Objects.Class Class(Type type)
            => new(this, type);

        internal LIR.TypeHandle Handle(Type type)
        {
            if (!types.TryGetValue(type, out var handle))
            {
                types[type] = handle = new();
                runtime.Map(handle, type);
            }

            return handle;
        }

        internal LIR.FunctionHandle Handle(MethodBase method)
        {
            if (functions.TryGetValue(method, out var handle))
                return handle;

            var signature = new LIR.Signature();

            if (!method.IsStatic && method is MethodInfo)
                signature.Parameters.Add(new(Handle(method.DeclaringType!)) { Name = "this" });

            foreach (var parameter in method.GetParameters())
                signature.Parameters.Add(new(Handle(parameter.ParameterType)) { Name = parameter.Name });

            signature.ReturnType = ReturnType(method) is { } type ? Handle(type) : null;

            functions[method] = handle = new(signature);
            runtime.Map(handle, method);

            return handle;
        }

        internal LIR.FunctionHandle Handle(FieldInfo field, bool isSet)
        {
            if (fieldAccessors.TryGetValue((field, isSet), out var handle))
                return handle;

            var signature = new LIR.Signature();

            if (!field.IsStatic)
                signature.Parameters.Add(new(Handle(field.DeclaringType!)) { Name = "this" });

            if (isSet)
                signature.Parameters.Add(new(Handle(field.FieldType)) { Name = "value" });
            else
                signature.ReturnType = Handle(field.FieldType);

            fieldAccessors[(field, isSet)] = handle = new(signature);

            var opcode = (isSet, field.IsStatic) switch
            {
                (false, false) => System.Reflection.Emit.OpCodes.Ldfld,
                (false, true) => System.Reflection.Emit.OpCodes.Ldsfld,
                (true, false) => System.Reflection.Emit.OpCodes.Stfld,
                (true, true) => System.Reflection.Emit.OpCodes.Stsfld,
            };

            runtime.Map(handle, il => il.Emit(opcode, field));

            return handle;
        }

        internal static Type? ReturnType(MethodBase method)
            => method switch
            {
                ConstructorInfo constructor => constructor.DeclaringType,
                MethodInfo { ReturnType: var type } when type != typeof(void) => type,
                _ => null
            };

        private IResult LoadClass(Package.DotNet.Class node)
            => Result.Ok(Class(classTypes[node]));

        private IResult LoadMethod(Package.DotNet.Method node)
            => Result.Ok(new Objects.MethodGroup(this, [methodBases[node]], null));

        private IResult LoadExposed(Package.DotNet.ExposedObject node)
            => Result.Ok(new Objects.Literal(node.Value, classTypes[node.Type], this));
    }
}
