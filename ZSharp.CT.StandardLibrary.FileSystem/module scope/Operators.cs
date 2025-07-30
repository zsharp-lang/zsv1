namespace Standard.FileSystem
{
    [ModuleScope]
    public static class Operators
    {
        [Operator("/", Kind = OperatorKind.Infix)]
        public static Path Join(Path path, RawPath sub)
            => path.JoinWith(sub);

        [Operator("/", Kind = OperatorKind.Infix)]
        public static Path Join(Path path, RelativePath sub)
            => path.JoinWith(sub.raw);

        [Operator("/", Kind = OperatorKind.Infix)]
        public static Item Join(Directory directory, RelativePath path)
            => Item.From(System.IO.Path.Combine(directory.raw, path.raw));

        [Operator("/", Kind = OperatorKind.Infix)]
        public static Item Join(Directory directory, RawPath path)
            => Item.From(System.IO.Path.Combine(directory.raw, path));
    }
}
