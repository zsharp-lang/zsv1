global using CommonZ;

namespace ZSharp.Compiler
{
    /// <summary>
    /// Maps a node's exact type to the function that compiles it.
    /// </summary>
    public sealed class ASTMap<TNode, TResult>
        where TNode : AST.Node
        where TResult : class
    {
        private readonly Dictionary<Type, Func<TNode, IResult<TResult, Error>>> compilers = [];

        /// <summary>
        /// Registers a compiler, replacing any existing one for that type.
        /// </summary>
        /// <returns>
        /// The replaced compiler, which the new one may delegate to, or
        /// <see langword="null"/> if there was none.
        /// </returns>
        public Func<TNode, IResult<TResult, Error>>? Register<T>(Func<T, IResult<TResult, Error>> compiler)
            where T : TNode
        {
            compilers.TryGetValue(typeof(T), out var previous);

            compilers[typeof(T)] = node => compiler((T)node);

            return previous;
        }

        public IResult<TResult, Error> Compile(TNode node)
            => compilers.TryGetValue(node.GetType(), out var compiler)
                ? compiler(node)
                : Result<TResult>.Error(new UnsupportedError($"No compiler is registered for {node.GetType().Name}."));
    }
}
