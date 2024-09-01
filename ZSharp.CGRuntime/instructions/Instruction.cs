namespace ZSharp.CGRuntime.HLVM
{
    /// <summary>
    /// Base class for all code generator instructions.
    /// 
    /// CG instructions are instructions that represent the different protocols an
    /// object can implement to be used in the code generator.
    /// 
    /// The CG VM is a dynamically typed VM, so CG code does not contain type information.
    /// </summary>
    public abstract class Instruction
    {
        public override string ToString()
            => $"{GetType().Name}()";
    }
}
