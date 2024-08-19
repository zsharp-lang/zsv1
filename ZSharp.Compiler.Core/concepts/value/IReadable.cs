namespace ZSharp.CTRuntime
{
    public interface ICTReadable
    {
        public ZSType Type { get; }

        public Code Cast(ZSCompiler compiler, ZSType type)
            => this is ICTTypeCast typeCast ? typeCast.Cast(compiler, type) : throw new NotImplementedException();

        public Code Read(ZSCompiler compiler, ZSType? @as)
        {
            if (@as is null || @as == Type)
                return Read(compiler);
            return Cast(compiler, @as);
        }

        public Code Read(ZSCompiler compiler);
    }
}
