using ZSharp.Compiler;

namespace ZSharp.Objects
{
    /// <summary>
    /// Defines the base structure for a function.
    /// 
    /// A function must have a signature.
    /// The signature is used to be able to choose an overload at CG.
    /// 
    /// This is abstract because the behavior for calling a function is
    /// different for each type of function.
    /// </summary>
    public abstract class Function(string? name) 
        : CompilerObject
        , ICTCallable
    {


        public abstract CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments);
    }
}
