using System.Diagnostics.CodeAnalysis;

namespace Standard.FileSystem
{
    public abstract class Path(RawPath raw)
    {
        protected internal readonly RawPath raw = raw ?? throw new ArgumentNullException(nameof(raw));

        public bool Is([NotNullWhen(true)] out Directory? result)
            => Resolve().Is(out result);

        public bool Is([NotNullWhen(true)] out File? result)
            => Resolve().Is(out result);

        public abstract AbsolutePath Resolve();

        public abstract Path JoinWith(params RawPath[] raw);

        public override string ToString()
            => $"{GetType().Name}<{raw}>";

        public static Path? From(RawPath raw)
            => AbsolutePath.From(raw) as Path
            ?? RelativePath.From(raw);
    }
}
