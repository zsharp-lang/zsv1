namespace ZSharp.Compiler
{
    public interface ICTReadable
        : ITyped
    {
        public IRCode Cast(Compiler compiler, IType type)
            => this is ICTTypeCast typeCast 
            && typeCast.Cast(compiler, type) is ICTReadable readable
            ?  readable.Read(compiler, type)
            : throw new NotImplementedException();

        public IRCode Read(Compiler compiler, IType? @as)
        {
            if (@as is null || @as == Type)
                return Read(compiler);
            return Cast(compiler, @as);
        }

        public IRCode Read(Compiler compiler);
    }
}
