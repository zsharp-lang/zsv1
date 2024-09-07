using System.Diagnostics.CodeAnalysis;
using ZSharp.VM;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public Runtime Runtime { get; }

        public IR.RuntimeModule RuntimeModule => Runtime.RuntimeModule;

        public ZSObject Evaluate(CGObject @object)
        {
            var irCode = CompileIRCode(@object);

            var code = Runtime.Assemble(irCode.Instructions, stackSize: irCode.MaxStackSize);

            return Runtime.EvaluateInNewFrame(code);
        }

        public bool Evaluate(CGObject @object, [NotNullWhen(true)] out ZSObject? result)
        {
            var irCode = CompileIRCode(@object);

            var code = Runtime.Assemble(irCode.Instructions, stackSize: irCode.MaxStackSize);

            try
            {
                return (result = Runtime.EvaluateInNewFrame(code)) is not null;
            } catch (StackIsEmptyException)
            {
                return (result = null) is not null;
            }
        }

        public void Execute(CGObject @object)
        {
            var irCode = CompileIRCode(@object);

            var code = Runtime.Assemble(irCode.Instructions, stackSize: irCode.MaxStackSize);

            Runtime.ExecuteInNewFrame(code);
        }
    }
}
