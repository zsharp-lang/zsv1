global using CommonZ;

namespace ZSharp.Compiler
{
    /// <summary>
    /// Maps a node's exact type to the function that compiles it. Each function receives
    /// the compiler it runs in.
    /// </summary>
    public sealed class ASTMap<TNode, TResult>
        where TNode : AST.Node
        where TResult : class
    {
        private readonly Dictionary<Type, Func<ASTCompiler, TNode, IResult<TResult, Error>>> compilers = [];

        /// <summary>
        /// Registers a compiler, replacing any existing one for that type.
        /// </summary>
        /// <returns>
        /// The replaced compiler, which the new one may delegate to, or
        /// <see langword="null"/> if there was none.
        /// </returns>
        public Func<ASTCompiler, TNode, IResult<TResult, Error>>? Register<T>(Func<ASTCompiler, T, IResult<TResult, Error>> compiler)
            where T : TNode
        {
            compilers.TryGetValue(typeof(T), out var previous);

            compilers[typeof(T)] = (current, node) => compiler(current, (T)node);

            return previous;
        }

        public IResult<TResult, Error> Compile(ASTCompiler current, TNode node)
            => compilers.TryGetValue(node.GetType(), out var compiler)
                ? compiler(current, node)
                : Result<TResult>.Error(new UnsupportedError($"No compiler is registered for {node.GetType().Name}."));
    }
}
