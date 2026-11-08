namespace ZSharp.Lang
{
    /// <summary>
    /// A context with names: where a name is looked up and bound.
    /// </summary>
    public interface INames
    {
        public Scope Names { get; }
    }

    /// <summary>
    /// A context that builds a definition found inside the code it compiles. A
    /// definition that isn't cast is built with whatever builds that code, and the code
    /// refers to the result.
    /// </summary>
    public interface IBuildsDefinitions
    {
        public IResult<HIRExpression, Error> Build(CompiledDefinition definition, List<HIRExpression> decorators);
    }

    /// <summary>
    /// The context of a definition: it binds its own name inside itself.
    /// </summary>
    public sealed class DefinitionContext(Context? parent, Scope names)
        : ChildContext(parent)
        , INames
    {
        public Scope Names { get; } = names;
    }

    /// <summary>
    /// A block of code, which has its own names.
    /// </summary>
    public sealed class BlockContext(Context? parent, Scope names)
        : ChildContext(parent)
        , INames
    {
        public Scope Names { get; } = names;
    }

    /// <summary>
    /// Code built by a block, as a body is built by the code of the definition it
    /// belongs to, or a container's members by the container.
    /// </summary>
    public sealed class BuildContext(Context? parent, HIR.Code.Block block)
        : ChildContext(parent)
        , IBuildsDefinitions
    {
        public IResult<HIRExpression, Error> Build(CompiledDefinition definition, List<HIRExpression> decorators)
        {
            if (definition.Fill().When(out var fill).Error(out var error))
                return Result<HIRExpression>.Error(error);

            definition.Variable.Init = definition.Construct;
            block.Statements.Add(new HIR.Code.Declare(definition.Variable));
            block.Statements.Add(fill!);

            if (decorators.Count == 0)
                return Result<HIRExpression>.Ok(new HIR.Code.Get(definition.Variable));

            var decorated = Language.Decorate(new HIR.Code.Get(definition.Variable), decorators);
            var result = new HIR.Code.Local(definition.Variable.Name, HIR.Infer.Value) { Init = decorated };

            block.Statements.Add(new HIR.Code.Declare(result));

            return Result<HIRExpression>.Ok(new HIR.Code.Get(result));
        }
    }
}
