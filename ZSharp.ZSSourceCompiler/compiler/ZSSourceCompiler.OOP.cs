namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ZSSourceCompiler : Compiler.Feature
    {
        public Objects.ClassMetaClass DefaultMetaClass { get; set; } = new();
    }
}
