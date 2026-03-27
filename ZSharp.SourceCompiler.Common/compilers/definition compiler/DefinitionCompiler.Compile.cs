namespace ZSharp.SourceCompiler
{
    partial class DefinitionCompiler
    {
        public IResult<Tuple<HIR.Node, Action>, Error> Compile(AST.Definition node)
        {
            CompilerObject? metaType = null;

            if (node.MetaType is null)
            {
                metaType = DefaultMetaType(node.GetType());

                if (metaType is null)
                    return Result<Tuple<HIR.Node, Action>>.Error($"No default meta type found for definition of type {node.GetType().Name}");
            }
            else
            {
                if (
                    CompileExpression(node.MetaType)
                    .When(out metaType)
                    .Error(out var e)
                )
                    return Result<Tuple<HIR.Node, Action>>.Error(e);
            }

            LateConstructor lateConstructor = new();

            if (
                CreateBuildFunction(node, lateConstructor)
                .When(interpreter.ILLoader.Expose)
                .When(out var buildFunctionCO)
                .Error(out var error)
                ||
                compiler.CG.Call(
                    metaType!,
                    [
                        new(buildFunctionCO!)
                    ]
                )
                .When(out var callCO)
                .Error(out error)
                ||
                interpreter.Evaluate(callCO!)
                .When(out var metaTypeInstance)
                .Error(out error)
            ) return Result<Tuple<HIR.Node, Action>>.Error(error!);

            if (metaTypeInstance is not HIR.Node hirNode)
                return Result<Tuple<HIR.Node, Action>>.Error($"Meta type instance is not a HIR node, got {metaTypeInstance!.GetType().Name}");

            if (lateConstructor.ActualLateConstructor is null)
                return Result<Tuple<HIR.Node, Action>>.Error(
                    "Late constructor is null which means either the metaclass never called " +
                    "the build function or the build function is incorrect"
                );

            return Result<Tuple<HIR.Node, Action>>.Ok(new(hirNode, lateConstructor.ActualLateConstructor));
        }
    }
}
