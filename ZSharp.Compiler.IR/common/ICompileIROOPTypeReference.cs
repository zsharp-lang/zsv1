namespace ZSharp.Compiler
{
    public interface ICompileIROOPTypeReference<T> 
        : ICompileIRReference<OOPTypeReference>
        , ICompileIRReference<OOPTypeReference<T>>
        where T : OOPType
    {
        Result<OOPTypeReference> ICompileIRReference<OOPTypeReference>.CompileIRReference(IR ir)
            => (this as ICompileIRReference < OOPTypeReference < T >>).CompileIRReference(ir).When(v => v as OOPTypeReference);
    }
}
