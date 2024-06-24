
namespace ZSharp.CTRuntime
{
    public sealed class Class(IR.Class @class)
        : IRBinding<IR.Class>(@class, VM.TypeSystem.Type)
        , ICTBinding, IRTBinding
    {
        CTType ICTBinding.Type => Type;

        CTType IRTBinding.Type => Type;

        CTType ICTBinding.Read(ZSCompiler compiler)
        {
            return compiler.Interpreter.LoadIR(IR);
        }

        RTType IRTBinding.Read(ZSCompiler compiler)
        {
            return new(1, [ new IR.VM.GetObject(IR) ], Type);
        }

        CTType ICTBinding.Write(ZSCompiler compiler, ICTBinding value)
        {
            throw new NotImplementedException();
        }

        RTType IRTBinding.Write(ZSCompiler compiler, IRTBinding value)
        {
            throw new NotImplementedException();
        }
    }
}
