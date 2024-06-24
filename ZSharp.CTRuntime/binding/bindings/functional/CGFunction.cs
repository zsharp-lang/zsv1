namespace ZSharp.CTRuntime
{
    public sealed class CGFunction(IR.Function function, VM.ZSSignature signature)
        : IRBinding<IR.Function>(function, new CGFunctionType(signature))
        , IRTBinding, ICTBinding
    {
        CTType IRTBinding.Type => Type;

        CTType ICTBinding.Type => Type;

        RTType IRTBinding.Read(ZSCompiler compiler)
            => new(1, [new IR.VM.GetObject(IR)], Type);

        CTType ICTBinding.Read(ZSCompiler compiler)
            => compiler.Interpreter.LoadIR(IR);

        RTType IRTBinding.Write(ZSCompiler compiler, IRTBinding value)
            => throw new NotImplementedException("overloading = is not supported yet");

        CTType ICTBinding.Write(ZSCompiler compiler, ICTBinding value)
            => throw new NotImplementedException("overloading = is not supported yet");
    }
}
