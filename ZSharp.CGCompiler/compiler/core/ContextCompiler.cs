using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    internal abstract class ContextCompiler(Context context)
        : CompilerBase(context)
    {
        /// <summary>
        /// Compile the given definition node in the context of this compiler.
        /// </summary>
        /// <param name="definition">The definition to compile.</param>
        /// <returns>A <see cref="CGObject"/> instance of compilation is successfull,
        /// <see cref="null"/> otherwise.</returns>
        protected internal abstract CGObject? Compile(RDefinition definition);

        /// <summary>
        /// Compile the given statement in the context of this compiler.
        /// </summary>
        /// <param name="statement">The statement to compile.</param>
        /// <returns><see cref="true"/> if the compiler supports the type of the
        /// given statement, <see cref="false"/> otherwise.</returns>
        protected internal virtual bool Compile(RStatement statement)
        {
            return false;
        }

        /// <summary>
        /// Add CG code to the current context.
        /// </summary>
        /// <param name="code">The code to emit.</param>
        protected internal abstract void AddCode(CGCode code);
    }

    internal abstract class ContextCompiler<TNode, TResult>
        (Context context) : ContextCompiler(context)
        where TNode : RDefinition
        where TResult : CGObject
    {
        public TNode Node { get; private set; } = null!;

        public TResult Result { get; private set; } = null!;

        protected internal override CGObject? Compile(RDefinition definition)
        {
            if (definition is TNode node)
                return Compile(node);

            return null;
        }

        public TResult Compile(TNode node)
        {
            (Node, node) = (node, Node);

            TResult result = Create();

            Define(result);

            (Result, result) = (result, Result);

            using (
                new ContextManager(() =>
                {
                    Node = node; 
                    Result = result;
                })
            )
            using (Context.For(Result))
            {
                using (Context.Of(this))
                    Compile(Result);

                return Result;
            }
        }

        protected abstract TResult Create();

        protected virtual void Define(TResult @object) { }

        protected abstract void Compile(TResult @object);
    }
}
