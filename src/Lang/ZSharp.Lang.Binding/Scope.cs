namespace ZSharp.Lang
{
    /// <summary>
    /// Names in source, and the code each stands for. It only stores them: binding a
    /// name goes through <see cref="Binder"/>.
    /// </summary>
    public sealed class Scope(Scope? parent = null)
    {
        private readonly Dictionary<string, HIR.Code.Expression> names = [];

        public Scope? Parent { get; } = parent;

        public void Add(string name, HIR.Code.Expression value)
            => names[name] = value;

        /// <summary>
        /// What a name stands for in this scope only.
        /// </summary>
        public HIR.Code.Expression? Local(string name)
            => names.TryGetValue(name, out var value) ? value : null;

        /// <summary>
        /// What a name stands for here or in an outer scope.
        /// </summary>
        public HIR.Code.Expression? Get(string name)
            => Local(name) ?? Parent?.Get(name);
    }
}
