namespace ZSharp.Compiler
{
    public interface ICTReadable
    {
        public CGObject Type { get; }

        public Code Cast(Compiler compiler, CGObject type)
            => this is ICTTypeCast typeCast 
            && typeCast.Cast(compiler, type) is ICTReadable readable
            ?  readable.Read(compiler, type)
            : throw new NotImplementedException();

        public Code Read(Compiler compiler, CGObject? @as)
        {
            if (@as is null || @as == Type)
                return Read(compiler);
            return Cast(compiler, @as);
        }

        public Code Read(Compiler compiler);
    }
}
