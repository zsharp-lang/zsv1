namespace ZSharp.Compiler.ILLoader
{
    public interface ITypeModifier
    {
        public CompilerObject Modify(CompilerObject type);
    }
}
