using CommonZ.Utils;

namespace ZSharp.Compiler
{
    public delegate bool InferenceTargetResolver(InferenceTarget target);

    public sealed class InferenceScope(Compiler compiler, InferenceScope? parent)
        : IDisposable
    {
        public Compiler Compiler { get; } = compiler;

        public InferenceScope? Parent { get; } = parent;

        public required InferenceTargetResolver DefaultResolver { get; set; }

        public Collection<InferenceTarget> Targets { get; private set; } = [];

        public void Dispose()
        {
            if (!Resolve())
                PushToParent();
        }

        public bool Resolve()
        {
            Collection<InferenceTarget> notResolved = [];

            foreach (var target in Targets)
                if (!(target.Resolve ?? DefaultResolver)(target))
                    notResolved.Add(target);

            return (Targets = notResolved).Count > 0;
        }

        public void PushToParent()
        {
            Parent?.Targets.AddRange(Targets);

            Targets.Clear();
        }
    }
}
