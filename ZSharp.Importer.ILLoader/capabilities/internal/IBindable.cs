namespace ZSharp.Importer.ILLoader
{
    internal interface IBindable
    {
        public Result Bind(Compiler.Compiler compiler, CompilerObject target);
    }
}
