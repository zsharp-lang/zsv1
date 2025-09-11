namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed partial class Global
        : CompilerObject
    {
        public string Name => IL.Name;

        public Global(IL.FieldInfo il, ILLoader loader)
        {
            IL = il;
            Type = loader.LoadType(il.FieldType);
        }
    }
}
