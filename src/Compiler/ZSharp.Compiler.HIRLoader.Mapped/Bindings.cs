namespace ZSharp.Compiler

{

    /// <summary>

    /// Binds a node to what it was built as, as an importer that gives a node of a built

    /// definition does, so loading it gives that and not a new definition.

    /// </summary>

    public delegate Error? Binder<in T>(T node, object entity)

        where T : class;



    /// <summary>

    /// Maps a node's exact type to the function that binds it to what it was built as.

    /// </summary>

    public sealed class Bindings
    {
        private readonly Dictionary<Type, Func<object, object, Error?>> binders = [];

        private readonly List<(object Node, object Entity)> waiting = [];

        /// <summary>
        /// Registers a binder, replacing any for that type, and binds the nodes of that type
        /// that were waiting for it.
        /// </summary>
        public Error? Register<T>(Binder<T> binder)
            where T : class
        {
            binders[typeof(T)] = (node, entity) => binder((T)node, entity);

            foreach (var (node, entity) in waiting.Where(pair => pair.Node.GetType() == typeof(T)).ToList())
            {
                waiting.Remove((node, entity));

                if (binder((T)node, entity) is { } error)
                    return error;
            }

            return null;
        }

        /// <summary>
        /// Whether a node of this type can be bound now.
        /// </summary>
        public bool Binds(object node)
            => binders.ContainsKey(node.GetType());

        /// <summary>
        /// Binds a node to what it was built as, or tells why it can't be. A node whose kind
        /// has no binder yet waits for one, as a library's nodes can be imported before the
        /// library that binds them.
        /// </summary>
        public Error? Bind(object node, object entity)
        {
            if (binders.TryGetValue(node.GetType(), out var binder))
                return binder(node, entity);

            waiting.Add((node, entity));
            return null;
        }
    }
}
