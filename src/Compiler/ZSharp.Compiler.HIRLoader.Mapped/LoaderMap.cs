namespace ZSharp.Compiler
{
    public delegate IResult Loader<in T>(T node)
        where T : class;

    /// <summary>
    /// Maps a node's exact type to the function that loads it.
    /// </summary>
    public sealed class LoaderMap<TNode>
        where TNode : class
    {
        private readonly Dictionary<Type, Loader<TNode>> loaders = [];

        /// <summary>
        /// Registers a loader, replacing any existing one for that type.
        /// </summary>
        /// <returns>
        /// The replaced loader, which the new one may delegate to, or
        /// <see langword="null"/> if there was none.
        /// </returns>
        public Loader<T>? Register<T>(Loader<T> loader)
            where T : class, TNode
        {
            loaders.TryGetValue(typeof(T), out var previous);

            loaders[typeof(T)] = node => loader((T)node);

            return previous;
        }

        public IResult Load(TNode node)
            => loaders.TryGetValue(node.GetType(), out var loader)
                ? loader(node)
                : Result.Error(new UnsupportedError($"No loader is registered for {node.GetType().Name}."));
    }
}
