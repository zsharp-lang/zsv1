namespace ZSharp.Compiler
{
    public interface ICTReadable
    {
        public CTObject Type { get; }

        public IRCode Cast(ZSCompiler compiler, CTObject type)
            => this is ICTTypeCast typeCast 
            && typeCast.Cast(compiler, type) is ICTReadable readable
            ?  readable.Read(compiler, type)
            : throw new NotImplementedException();

        public IRCode Read(ZSCompiler compiler, CTObject? @as)
        {
            if (@as is null || @as == Type)
                return Read(compiler);
            return Cast(compiler, @as);
        }

        public IRCode Read(ZSCompiler compiler);
    }
}
