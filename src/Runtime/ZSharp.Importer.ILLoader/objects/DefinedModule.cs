namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A module defined in Z#, of the CLR's meta-type: its classes, its functions and its
    /// <c>let</c>s. It is lowered for each CLR platform it is used on, to an LIR module of
    /// that platform's, as a class defined in Z# is.
    /// </summary>
    internal sealed class DefinedModule(ILLoader loader, Package.DotNet.Module node)
        : CompilerObject
        , ICTGetMember<MemberName>
        , ICTCastTo
    {
        public Package.DotNet.Module Node { get; } = node;

        public Dictionary<object, LoweredModule> Lowerings { get; } = [];

        public IResult<LoweredModule, Error> For(object? target)
            => loader.Lower(this, target);

        /// <summary>
        /// Casting a module to what describes it is reflection, as for a type.
        /// </summary>
        public IResult Cast(CompilerObject targetType)
            => Core.Compiler.ModuleScope.TS.IsAssignableTo(targetType, loader.Class(Node.GetType()))
                ? Result.Ok(new Literal(Node, Node.GetType(), loader))
                : Result.Error(new UnsupportedError("A module can't be cast to that type."));

        public IResult Member(MemberName member)
        {
            // A class or an interface is found as a type.
            if (Node.Definitions.OfType<Package.DotNet.TypeReference>().FirstOrDefault(type => Named(type, member)) is { } found)
                return Core.Compiler.ModuleScope.HIRLoader.LoadType(found);

            var functions = Node.Methods
                .Where(method => method.Name == member)
                .Select(method => new ModuleFunction(loader, method, this))
                .ToArray<CompilerObject>();

            if (functions.Length > 0)
                return Result.Ok(OverloadGroup.Of(functions));

            if (Node.Fields.FirstOrDefault(field => field.Name == member) is { } field)
                return Result.Ok(new ModuleGlobal(this, field));

            return Result.Error(new UnsupportedError($"Module {Node.Name} has no member named {member}."));
        }

        private static bool Named(Package.DotNet.TypeReference type, string name)
            => type switch
            {
                Package.DotNet.Class @class => @class.Name == name,
                Package.DotNet.Interface @interface => @interface.Name == name,
                _ => false
            };
    }

    /// <summary>
    /// A defined module as one target has it: its LIR module, and its members in it.
    /// </summary>
    internal sealed class LoweredModule(LIR.Module module)
    {
        public LIR.Module Module { get; } = module;

        public Dictionary<Package.DotNet.Method, LIR.Function> Functions { get; } = [];

        public Dictionary<Package.DotNet.Field, LIR.Global> Globals { get; } = [];
    }
}
