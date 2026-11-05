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

        private readonly Dictionary<MethodInfo, Package.DotNet.Method> methods = [];
        private readonly Dictionary<Package.DotNet.Method, LIR.FunctionHandle> functions = [];

        public void Install(MappedLoader loader, MappedCodeLoader code)
        {
            loader.Instances.Register<Package.DotNet.Method>(LoadMethod);
            loader.Definitions.Register<Package.DotNet.Method>(LoadMethod);

            loader.Instances.Register<Package.DotNet.Class>(LoadClass);
            loader.Definitions.Register<Package.DotNet.Class>(LoadClass);
            loader.Types.Register<Package.DotNet.Class>(LoadClass);

            code.Expressions.Register<HIR.Code.StringLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, Handle(typeof(string))))
            );
            code.Expressions.Register<HIR.Code.BooleanLiteral>(literal =>
                Result.Ok(new Objects.Literal(literal.Value, Handle(typeof(bool))))
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

        public Package.DotNet.Method Load(MethodInfo method)
        {
            if (!methods.TryGetValue(method, out var result))
            {
                methods[method] = result = new(method.Name);
                functions[result] = Handle(method);
            }

            return result;
        }

        internal IResult LoadClass(Package.DotNet.Class node)
            => Result.Ok(new Objects.Class(this, classTypes[node], Handle(classTypes[node])));

        internal IResult LoadMethod(Package.DotNet.Method node)
            => Result.Ok(new Objects.Method(functions[node]));

        private LIR.TypeHandle Handle(Type type)
        {
            if (!types.TryGetValue(type, out var handle))
            {
                types[type] = handle = new();
                runtime.Map(handle, type);
            }

            return handle;
        }

        private LIR.FunctionHandle Handle(MethodInfo method)
        {
            var signature = new LIR.Signature
            {
                ReturnType = method.ReturnType == typeof(void) ? null : Handle(method.ReturnType)
            };

            foreach (var parameter in method.GetParameters())
                signature.Parameters.Add(new(Handle(parameter.ParameterType)) { Name = parameter.Name });

            var handle = new LIR.FunctionHandle(signature);

            runtime.Map(handle, method);

            return handle;
        }
    }
}
