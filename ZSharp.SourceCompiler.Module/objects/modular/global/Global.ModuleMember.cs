
namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Global
        : IModuleMember
    {
        public Module Module { get; internal set; }

        CompilerObject IModuleMember.Module => Module;
    }
}
