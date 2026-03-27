using ZSharp.SourceCompiler.Objects;

namespace ZSharp.SourceCompiler
{
    public static partial class TwoPass
    {
        public static TwoPassPrepared Prepare(List<AST.Expression> definitions)
        {
            return new()
            {
                Tasks = definitions.Select(
                    def =>
                    {
                        // create a build function
                        // expose it to get a CO
                        // compile the metaclass expression to CO
                        // use Call on the metaclass and the exposed build function
                        // evaluate the result CO to an object
                        // make sure the object is an HIR.Node
                        // 
                        // To make the build function we need to also create
                        // a function that takes the contents and returns an HIR list.
                        // Oh, it's actually Prepare + Perform.
                        // Now the only thing left is to construct a spec and call
                        // the late constructor with it.
                        //
                        // Each definition type has a different build function because
                        // each build function accepts a different late constructor.

                        CompilerObject? build = null;
                        CompilerObject? mc = null;
                        Action? lateConstructor = null;

                        // Create and expose the build function
                        switch (def)
                        {
                            case AST.OOPDefinition @oop:
                            {
                                mc = Compile(oop.Of ?? DefaultMetaclass);
                                build = Expose(
                                    (Action<ClassSpecification> constructor) =>
                                    {
                                        lateConstructor = () =>
                                        {
                                            constructor(CreateSpecification(oop));
                                        };
                                    }
                                );
                                break;
                            }
                            case AST.Function function:
                            {
                                break;
                            }
                            case AST.Module module:
                            {
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }

                        if (build is null || mc is null || lateConstructor is null)
                            throw new("Uh no! Why the hell is this an exception???");

                        // Call the metaclass. Oh, we need the compiler
                        var mcCallResult = compiler.CG.Call(
                            mc,
                            [
                                new(build)
                            ]
                        );

                        if (mcCallResult
                            .When(out var mcCall)
                            .Error(out var error)
                        ) return Result.Error(error);

                        // Evaluate the result CO to an object. Now we need an evaluator.
                        var evaluatedResult = compiler.Evaluate(mcCall);

                        if (evaluatedResult
                            .When(out var obj)
                            .Error(out var evalError)
                        ) return Result.Error(evalError);

                        // Make sure the object is an HIR.Node. Ok, here we just use `is`.
                        if (obj is not HIR.Node node)
                            return Result.Error(
                                $"The metaclass constructor " + 
                                $"for {def} did not return an HIR.Node. It returned {obj}."
                            );

                        // Now we need to create a slot? Hm, maybe not.

                // If we return here it means the caller does whatever it wants
                // whether is registering or whatever.

                        return () => lateConstructor();
                    }
                )
            };
        }
    }
}
