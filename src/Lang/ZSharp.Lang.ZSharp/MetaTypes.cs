namespace ZSharp.Lang
{
    /// <param name="definition">The definition to compile.</param>
    /// <param name="variable">
    /// The local that will hold the object, typed as the meta-type. The meta-type is
    /// also used for reflection, which can't mutate, so a meta-compiler may change the
    /// local's type to something editable.
    /// </param>
    public delegate IResult<CompiledDefinition, Error> MetaCompiler(
        AST.Definition definition,
        HIR.Code.Local variable
    );

    /// <summary>
    /// A map from a meta-type to its compiler. It is a utility, and nothing depends on
    /// it: anything registered for a definition node may compile it some other way. It
    /// resolves the meta-type, declares the local and binds the definition's name
    /// inside itself, and the rest is the meta-compiler's.
    /// </summary>
    /// <param name="resolve">Evaluates an <c>of</c> expression to its HIR object.</param>
    public sealed class MetaTypes(
        Language language,
        ASTCompiler compiler,
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
        }

        private IResult<CompiledDefinition, Error> Compile(AST.Definition definition)
        {
            if (MetaType(definition).When(out var metaType).Error(out var error))
                return Result<CompiledDefinition>.Error(error);

            if (!compilers.TryGetValue(metaType!, out var metaCompiler))
                return Result<CompiledDefinition>.Error($"No meta-compiler is registered for the meta-type of {NameOf(definition)}.");

            if (metaType is not HIR.Type type)
                return Result<CompiledDefinition>.Error($"The meta-type of {NameOf(definition)} is not a type.");

            var name = NameOf(definition);
            var variable = new HIR.Code.Local(name, type);

            var scope = new Scope(language.Scope);

            if (name.Length > 0)
                scope.Add(name, new HIR.Code.Get(variable));

            if (Within(scope, () => metaCompiler(definition, variable)).When(out var compiled).Error(out error))
                return Result<CompiledDefinition>.Error(error);

            return Result<CompiledDefinition>.Ok(compiled! with
            {
                Fill = () => Within(scope, compiled.Fill),
                Name = name
            });
        }

        private IResult<HIR.Instance, Error> MetaType(AST.Definition definition)
        {
            var of = definition is AST.TypeDefinition type ? type.Of : definition.MetaType;

            if (of is not null)
                return compiler.CompileExpression(of).When(out var expression).Error(out var error)
                    ? Result<HIR.Instance>.Error(error)
                    : resolve(expression!);

            return defaults.TryGetValue(definition.GetType(), out var metaType)
                ? Result<HIR.Instance>.Ok(metaType)
                : Result<HIR.Instance>.Error($"A {definition.GetType().Name} has no default meta-type.");
        }

        private T Within<T>(Scope scope, Func<T> compile)
        {
            var outer = language.Scope;
            language.Scope = scope;

            try
            {
                return compile();
            }
            finally
            {
                language.Scope = outer;
            }
        }

        private static string NameOf(AST.Definition definition)
            => definition switch
            {
                AST.TypeDefinition type => type.Name,
                AST.Module module => module.Name,
                _ => string.Empty
            };
    }
}
