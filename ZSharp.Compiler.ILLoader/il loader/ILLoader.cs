namespace ZSharp.Compiler.ILLoader
{
    public sealed partial class ILLoader
    {
        public Compiler Compiler { get; }

        public ILLoader(Compiler compiler)
        {
            Compiler = compiler;

            foreach (var (il, co) in (IEnumerable<(Type, IType)>)[
                (typeof(void), Compiler.TypeSystem.Void),
            ])
            {
                typeCache[il] = co;
            }
        }
    }
}
