namespace ZSharp.SourceCompiler
{
    partial class Compilers
    {
        public static HIR.BuiltIn.Module Compile(AST.Module node)
        {
            // NOTE: evaluate = compile + execute

            // First, we need a new slot of type Module.

            // Next, we need to enter a new scope.

            // Then, we need to iterate over the body.

            // Module body is a list of statements.

            // We evaluate each statement in the body.

            // When we encounter a definition, either directly in a 
            // definition statement or in an expression, we need to
            // evaluate a call on the metaclass. The metaclass is
            // called with a build function. This build function adds
            // a given callable to the late constructors list.

            // The call to the metaclass should return an object that
            // is an instance of `Node`. We make a new CT slot with
            // that node instance.

            // We do this for each definition.

            // Once we're done with that, we can start calling the 
            // late constructors. Before calling the constructor, we
            // need to create a spec. For that we need to compile the
            // body of that definition.

            // Ok, after calling all the late constructors we can take
            // all the nodes that we created, create the module spec and
            // call the Module constructor with that spec.

            // Now you might see that this construction appears twice.
            // Yeah, we'll need to refactor this out. Somehow.

            // Ok, now I realize 2 things:
            // 1. The process is just List<AST.Definition> -> List<HIR.Node>
            // 2. The process can be split:
            //    a. List<AST.Definition> -> List<(HIR.Node, Action)> = List of late constructors
            //    b. List<Action> -> List<HIR.Node> = call the late constructors

            // So we either do CompileTogether or we do Prepare + Perform.
        }
    }
}
