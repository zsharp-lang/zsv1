namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed partial class Module
        : CompilerObject
    {
        public string Name => IL.Name;

        public Module(IL.Module il)
        {
            IL = il;
            Globals = IL.GetType("<Module>");
        }
    }
}
