namespace ZSharp.Importer.ILLoader
{
    internal interface IBindable
    {
        public IResult Bind(Compiler.Compiler compiler, CompilerObject target);
    }
}
