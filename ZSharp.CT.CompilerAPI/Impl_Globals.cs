using ZSharp.Runtime.NET.IL2IR;

namespace ZS.CompilerAPI
{
    [ModuleGlobals]
    public static class Impl_Globals
    {
        public static ZSharp.Compiler.Compiler GetCompiler() => Fields_Globals.compiler;
    }
}
