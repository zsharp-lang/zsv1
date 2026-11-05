namespace ZSharp.Lang
{
    /// <summary>
    /// Names in Z# source, and the code each stands for.
    /// </summary>
    public sealed class Scope(Scope? parent = null)
    {
        private readonly Dictionary<string, HIRExpression> names = [];

        public Scope? Parent { get; } = parent;

        public void Add(string name, HIRExpression value)
            => names[name] = value;

        public HIRExpression? Get(string name)
            => names.TryGetValue(name, out var value) ? value : Parent?.Get(name);
    }
}
