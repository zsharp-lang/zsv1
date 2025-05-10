using CommonZ.Utils;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class Context
    {
        private readonly Mapping<CompilerObject, ScopeContext> objectContainerScopes = [];
        private readonly Mapping<CompilerObject, ScopeContext> objectContainedScopes = [];

        public ScopeContext GlobalScope { get; }

        public ScopeContext CurrentScope { get; private set; }

        public ScopeContext CreateScope()
            => new();

        public ContextManager Scope()
            => Scope(CreateScope());

        public ContextManager Scope(ScopeContext scope)
        {
            (CurrentScope, scope) = (scope, CurrentScope);
            var revert = CurrentCompiler.Compiler.Compiler.UseContext(CurrentScope);

            return new(() =>
            {
                CurrentScope = scope;
                revert();
            });
        }

        public ContextManager Scope(CompilerObject @object)
            => Scope(ContainedScope(@object) ?? ContainedScope(@object, CreateScope())); // TODO: proper exception: could not find scope for object

        public ScopeContext? ContainerScope(CompilerObject @object)
            => objectContainerScopes.GetValueOrDefault(@object);

        public ScopeContext? ContainedScope(CompilerObject @object)
            => objectContainedScopes.GetValueOrDefault(@object);

        public ScopeContext ContainedScope(CompilerObject @object, ScopeContext scope)
            => objectContainedScopes[@object] = scope;
    }
}
