namespace ZSharp.Lang
{
    /// <param name="compiler">The compiler the definition is compiled with.</param>
    /// <param name="definition">The definition to compile.</param>
    /// <param name="variable">
    /// The local that will hold the object, typed as the meta-type. The meta-type is
    /// also used for reflection, which can't mutate, so a meta-compiler may change the
    /// local's type to something editable.
    /// </param>
    public delegate IResult<CompiledDefinition, Error> MetaCompiler(
        ASTCompiler compiler,
        AST.Definition definition,
        HIR.Code.Local variable
    );

    /// <summary>
    /// A map from a meta-type to its compiler. It is a utility, and nothing depends on
    /// it: anything registered for a definition node may compile it some other way. It
    /// resolves the meta-type, declares the local, and gives the definition a context
    /// that binds its name inside itself. The rest is the meta-compiler's.
    /// </summary>
    /// <param name="resolve">Evaluates an <c>of</c> expression to its HIR object.</param>
    public sealed class MetaTypes(
        Language language,
        Func<HIRExpression, IResult<HIR.Instance, Error>> resolve
    )
    {
        private readonly Dictionary<HIR.Instance, MetaCompiler> compilers
            = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<Type, HIR.Instance> defaults = [];

        public void Register(HIR.Instance metaType, MetaCompiler compiler)
            => compilers[metaType] = compiler;

        /// <summary>
        /// Sets the meta-type of a kind of definition that has no <c>of</c>.
        /// </summary>
        public void Default<T>(HIR.Instance metaType)
            where T : AST.Definition
            => defaults[typeof(T)] = metaType;

        public void Install(MappedASTCompiler mapped)
        {
            mapped.Definitions.Register<AST.TypeDefinition>(Compile);
            mapped.Definitions.Register<AST.Module>(Compile);
            mapped.Definitions.Register<AST.Function>(Compile);
            mapped.Definitions.Register<AST.Constructor>(Compile);
        }

        private IResult<CompiledDefinition, Error> Compile(ASTCompiler compiler, AST.Definition definition)
            => MetaType(compiler, definition).When(out var metaType).Error(out var error)
                ? Result<CompiledDefinition>.Error(error)
                : Compile(compiler, definition, metaType!);

        /// <summary>
        /// Compiles a definition with the given meta-type, whatever its own would be. A
        /// meta-compiler that owns where a definition is can choose its meta-type this way.
        /// </summary>
        public IResult<CompiledDefinition, Error> Compile(ASTCompiler compiler, AST.Definition definition, HIR.Instance metaType)
        {
            Error? error;

            if (!compilers.TryGetValue(metaType, out var metaCompiler))
                return Result<CompiledDefinition>.Error($"No meta-compiler is registered for the meta-type of {NameOf(definition)}.");

            if (metaType is not HIR.Type type)
                return Result<CompiledDefinition>.Error($"The meta-type of {NameOf(definition)} is not a type.");

            var name = NameOf(definition);
            var variable = new HIR.Code.Local(name, type);

            var context = new DefinitionContext(compiler.Contexts.Current, new Scope(Language.Names(compiler)));

            if (name.Length > 0 && language.Bind(context.Names, name, new HIR.Code.Get(variable)) is { } invalid)
                return Result<CompiledDefinition>.Error(invalid);

            if (compiler.Contexts.Within(context, () => metaCompiler(compiler, definition, variable)).When(out var compiled).Error(out error))
                return Result<CompiledDefinition>.Error(error);

            return Result<CompiledDefinition>.Ok(compiled! with
            {
                Fill = () => compiler.Contexts.Within(context, compiled.Fill),
                Name = name
            });
        }

        /// <summary>
        /// Only a type definition can say its meta-type with <c>of</c>.
        /// </summary>
        private IResult<HIR.Instance, Error> MetaType(ASTCompiler compiler, AST.Definition definition)
        {
            if (definition is AST.TypeDefinition { Of: { } of })
                return compiler.CompileExpression(of).When(out var expression).Error(out var error)
                    ? Result<HIR.Instance>.Error(error)
                    : resolve(expression!);

            return defaults.TryGetValue(definition.GetType(), out var metaType)
                ? Result<HIR.Instance>.Ok(metaType)
                : Result<HIR.Instance>.Error($"A {definition.GetType().Name} has no default meta-type.");
        }

        private static string NameOf(AST.Definition definition)
            => definition switch
            {
                AST.TypeDefinition type => type.Name,
                AST.Module module => module.Name,
                AST.Function function => function.Name ?? string.Empty,
                _ => string.Empty
            };
    }
}
