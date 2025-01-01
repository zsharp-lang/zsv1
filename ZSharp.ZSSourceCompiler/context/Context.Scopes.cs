using CommonZ.Utils;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class Context
    {
        private readonly Mapping<CompilerObject, Scope> objectContainerScopes = [];
        private readonly Mapping<CompilerObject, Scope> objectContainedScopes = [];

        public Scope GlobalScope { get; }

        public Scope CurrentScope { get; private set; }

        public Scope CreateScope(ScopeParent parent = ScopeParent.Default)
            => new(parent switch
            {
                ScopeParent.None => null,
                ScopeParent.Global => GlobalScope,
                ScopeParent.Current => CurrentScope,
                _ => throw new ArgumentOutOfRangeException(nameof(parent), parent, $"Unknown {nameof(ScopeParent)} value")
            });

        public ContextManager Scope(ScopeParent parent = ScopeParent.Default)
            => Scope(CreateScope(parent));

        public ContextManager Scope(Scope scope)
        {
            (CurrentScope, scope) = (scope, CurrentScope);

            return new(() => CurrentScope = scope);
        }

        public ContextManager Scope(CompilerObject @object, ScopeParent parent = ScopeParent.Default)
            => Scope(ContainedScope(@object) ?? ContainedScope(@object, CreateScope(parent))); // TODO: proper exception: could not find scope for object

        public Scope? ContainerScope(CompilerObject @object)
            => objectContainerScopes.GetValueOrDefault(@object);

        public Scope? ContainedScope(CompilerObject @object)
            => objectContainedScopes.GetValueOrDefault(@object);

        public Scope ContainedScope(CompilerObject @object, Scope scope)
            => objectContainedScopes[@object] = scope;
    }
}
