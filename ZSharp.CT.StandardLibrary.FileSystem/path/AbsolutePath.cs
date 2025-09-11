using System.Diagnostics.CodeAnalysis;

namespace Standard.FileSystem
{
    public sealed class AbsolutePath : Path
    {
        private AbsolutePath(RawPath raw) : base(raw)
        {

        }

        public new bool Is([NotNullWhen(true)] out Directory? result)
            => (result = Directory.From(raw)) is not null;

        public new bool Is([NotNullWhen(true)] out File? result)
            => (result = File.From(raw)) is not null;

        public override AbsolutePath Resolve() 
            => this;

        public override Path JoinWith(params RawPath[] raw)
            => new AbsolutePath(System.IO.Path.Combine([this.raw, ..raw]));

        public new static AbsolutePath? From(RawPath raw)
        {
            if (!System.IO.Path.IsPathRooted(raw))
                return null;

            return new(raw);
        }
    }
}
