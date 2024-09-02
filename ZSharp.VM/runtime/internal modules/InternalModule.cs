using CommonZ.Utils;

namespace ZSharp.VM
{
    /// <summary>
    /// Defines a module whose functions are implemented in C# rather than Z#.
    /// </summary>
    /// <param name="moduleIR">An <see cref="IR.Module"/> that defines the structure of the module.</param>
    public abstract class InternalModule
    {
        public Mapping<IR.Function, ZSInternalFunctionDelegate> FunctionImplementations { get; } = [];

        internal protected abstract IR.Module Load(Runtime runtime);
    }
}
