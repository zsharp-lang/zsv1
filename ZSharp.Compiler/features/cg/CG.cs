namespace ZSharp.Compiler
{
    public readonly partial struct CG(Compiler compiler)
    {
        public readonly Compiler compiler = compiler;

        private bool This(out CG cg)
        {
            cg = this;
            return true;
        }
    }
}
