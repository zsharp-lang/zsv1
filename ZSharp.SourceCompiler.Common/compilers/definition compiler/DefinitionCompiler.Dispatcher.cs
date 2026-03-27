namespace ZSharp.SourceCompiler
{
    public delegate IResult<object, Error> DefinitionBuilderFactory(
        AST.Definition definition,
        LateConstructor build
    );

    public delegate IResult<object, Error> DefinitionBuilderFactory<T>(
        T definition,
        LateConstructor build
    ) where T : AST.Definition;

    partial class DefinitionCompiler
    {
        private readonly Dictionary<Type, DefinitionBuilderFactory> handlers = [];

        public void RegisterHandler<T>(DefinitionBuilderFactory<T> handler)
            where T : AST.Definition
        {
            handlers[typeof(T)] = (AST.Definition def, LateConstructor build) => handler((T)def, build);
        }

        internal IResult<object, Error> CreateBuildFunction(AST.Definition definition, LateConstructor lateConstructor)
        {
            if (!handlers.TryGetValue(definition.GetType(), out var factory))
                return Result<Action>.Error($"No handler registered for definition type: {definition.GetType().Name}");
            
            return factory(definition, lateConstructor);
        }
    }
}
