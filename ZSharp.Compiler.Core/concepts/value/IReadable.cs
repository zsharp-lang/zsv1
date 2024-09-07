namespace ZSharp.Compiler
{
    public interface ICTReadable
    {
        public IRType Type { get; }

        public Code Cast(ZSCompiler compiler, CGObject type)
            => this is ICTTypeCast typeCast 
            && typeCast.Cast(compiler, type) is ICTReadable readable
            ?  readable.Read(compiler, type)
            : throw new NotImplementedException();

        public Code Read(ZSCompiler compiler, CGObject? @as)
        {
            if (@as is null || @as == Type)
                return Read(compiler);
            return Cast(compiler, @as);
        }

        public Code Read(ZSCompiler compiler);
    }
}
