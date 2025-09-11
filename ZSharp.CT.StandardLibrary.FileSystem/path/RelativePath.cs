namespace Standard.FileSystem
{
    public sealed class RelativePath : Path
    {
        private RelativePath(RawPath raw) : base(raw)
        {

        }

        public override AbsolutePath Resolve()
            => AbsolutePath.From(System.IO.Path.GetFullPath(raw))!;

        public override Path JoinWith(params RawPath[] raw)
            => new RelativePath(System.IO.Path.Combine([this.raw, .. raw]));

        public new static RelativePath? From(RawPath raw)
        {
            if (System.IO.Path.IsPathRooted(raw))
                return null;

            return new(raw);
        }
    }
}
