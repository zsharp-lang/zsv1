using CommonZ.Utils;

namespace ZSharp.VM
{
    /// <summary>
    /// Defines a module whose functions are implemented in C# rather than Z#.
    /// </summary>
    /// <param name="moduleIR">An <see cref="IR.Module"/> that defines the structure of the module.</param>
    public sealed class InternalModule(IR.Module moduleIR)
    {
        public IR.Module ModuleIR { get; } = moduleIR;

        public Mapping<IR.Function, ZSInternalFunctionDelegate> FunctionImplementations { get; } = [];
    }
}
