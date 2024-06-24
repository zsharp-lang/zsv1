namespace ZSharp.CTRuntime
{
    public sealed class Module(IR.Module module) 
        : IRBinding<IR.Module>(module, null!)
        , IRTBinding, ICTBinding
    {
        CTType IRTBinding.Type => Type;

        CTType ICTBinding.Type => Type;

        RTType IRTBinding.Read(ZSCompiler compiler)
            => new(1, [new IR.VM.GetObject(IR)], Type);

        CTType ICTBinding.Read(ZSCompiler compiler)
            => compiler.Interpreter.LoadIR(IR);

        RTType IRTBinding.Write(ZSCompiler compiler, IRTBinding value)
            => throw new("Cannot assign to module");

        CTType ICTBinding.Write(ZSCompiler compiler, ICTBinding value)
            => throw new("Cannot assign to module");
    }
}
