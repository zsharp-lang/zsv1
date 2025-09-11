namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Function
        : IModuleMember
    {
        public Module Module { get; internal set; }

        CompilerObject IModuleMember.Module => Module;
    }
}
