using CommonZ;
using ZSharp.Compiler;

namespace ZSharp.Lang
{
    /// <summary>
    /// Merges what a name stands for with something new bound to it in the same scope,
    /// or returns <see langword="null"/> if it can't.
    /// </summary>
    public delegate HIR.Code.Expression? Merge(HIR.Code.Expression existing, HIR.Code.Expression value);

    /// <summary>
    /// Decides what to bind when a name hides one from an outer scope, for example by
    /// copying its overloads, or returns <see langword="null"/> to bind the value as it is.
    /// </summary>
    public delegate HIR.Code.Expression? Shadow(HIR.Code.Expression shadowed, HIR.Code.Expression value);

    /// <summary>
    /// What happens to a name bound twice in one scope when nothing merges them.
    /// </summary>
    public enum Conflict
    {
        Shadow,
        Error,
    }

    /// <summary>
    /// Implements <see cref="Binder"/> through registered rules.
    /// </summary>
    public sealed class MappedBinder(Binder binder)
    {
        private readonly Binder binder = binder;

        private Binder @base = null!;

        public List<Merge> Merges { get; } = [];

        public List<Shadow> Shadows { get; } = [];

        public Conflict Conflict { get; set; } = Conflict.Error;

        public void Apply()
        {
            @base = binder.Clone();

            binder.Bind = Bind;
        }

        private Error? Bind(Scope scope, string name, HIR.Code.Expression value)
        {
            if (@base.Bind(scope, name, value) is var result && !result.IsUnsupported())
                return result;

            if (scope.Local(name) is { } existing)
            {
                foreach (var merge in Merges)
                    if (merge(existing, value) is { } merged)
                    {
                        scope.Add(name, merged);
                        return null;
                    }

                if (Conflict == Conflict.Error)
                    return new ErrorMessage($"{name} is already defined here.");
            }
            else if (scope.Parent?.Get(name) is { } shadowed)
                foreach (var shadow in Shadows)
                    if (shadow(shadowed, value) is { } replaced)
                    {
                        scope.Add(name, replaced);
                        return null;
                    }

            scope.Add(name, value);

            return null;
        }
    }
}
