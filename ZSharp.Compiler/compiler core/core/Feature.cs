namespace ZSharp.Compiler
{
    public abstract class Feature
    {
        public Compiler Compiler { get; }

        public Feature(Compiler compiler)
        {
            Compiler = compiler;

            compiler.Feature(this);
        }
    }
}
