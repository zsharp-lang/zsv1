namespace ZSharp.Compiler
{
    /// <summary>
    /// Maps a node's exact type to the function that reifies it.
    /// </summary>
    public sealed class ReifierMap<TNode>
        where TNode : class
    {
        private readonly Dictionary<Type, Func<TNode, IResult<HIR.Code.Expression, Error>>> reifiers = [];

        /// <summary>
        /// Registers a reifier, replacing any existing one for that type.
        /// </summary>
        /// <returns>
        /// The replaced reifier, which the new one may delegate to, or
        /// <see langword="null"/> if there was none.
        /// </returns>
        public Func<TNode, IResult<HIR.Code.Expression, Error>>? Register<T>(Func<T, IResult<HIR.Code.Expression, Error>> reifier)
            where T : class, TNode
        {
            reifiers.TryGetValue(typeof(T), out var previous);

            reifiers[typeof(T)] = node => reifier((T)node);

            return previous;
        }

        public IResult<HIR.Code.Expression, Error> Reify(TNode node)
            => reifiers.TryGetValue(node.GetType(), out var reifier)
                ? reifier(node)
                : Result<HIR.Code.Expression>.Error(new UnsupportedError($"No reifier is registered for {node.GetType().Name}."));
    }
}
