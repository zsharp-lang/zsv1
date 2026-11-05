namespace Core.Compiler
{
    /// <summary>
    /// The toolchain objects as seen through <c>core:compiler</c>. Z# reaches them
    /// by importing that module, and C# reaches them here.
    /// </summary>
    public static class ModuleScope
    {
        public static ZSharp.Compiler.CG CG { get; set; } = new();

        public static ZSharp.Compiler.TS TS { get; set; } = new();

        public static ZSharp.Compiler.HIRLoader HIRLoader { get; set; } = new();

        public static ZSharp.Compiler.HIRCodeLoader HIRCodeLoader { get; set; } = new();
    }
}
