using CommonZ;
using ZSharp.Compiler;

namespace ZSharp.Lang
{
    /// <summary>
    /// Binds a name in a scope to what it stands for.
    /// </summary>
    /// <returns>Why the name can't be bound, or <see langword="null"/>.</returns>
    public delegate Error? Bind(Scope scope, string name, HIR.Code.Expression value);

    /// <summary>
    /// Defines a name. What to bind is up to whoever defines it. How a name that is
    /// already bound, here or in an outer scope, is handled is up to this. Names exist
    /// only in source, so this works on HIR before any CO exists.
    /// </summary>
    public class Binder
    {
        public Bind Bind { get; set; } = (scope, name, value)
            => new UnsupportedError($"Can't bind the name {name}.");

        public Binder Clone() => (Binder)MemberwiseClone();
    }
}
