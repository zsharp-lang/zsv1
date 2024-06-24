namespace ZSharp.CTRuntime
{
    public sealed class Global(IR.Global global, ZSType type) 
        : IRBinding<IR.Global>(global, type)
        , ICTBinding, IRTBinding
    {
        CTType ICTBinding.Read(ZSCompiler compiler)
        {
            if (IR.Module is null)
                throw new InvalidOperationException("Global variable has not been defined");

            var module = compiler.Interpreter.LoadIR<VM.ZSModule>(IR.Module!);
            return module.GetGlobal(IR);
        }

        CTType ICTBinding.Write(ZSCompiler compiler, ICTBinding value)
        {
            if (IR.Module is null)
                throw new InvalidOperationException("Global variable has not been defined");

            var module = compiler.Interpreter.LoadIR<VM.ZSModule>(IR.Module!);
            var result = value.Read(compiler);
            module.SetGlobal(IR, result);
            return result;
        }

        RTType IRTBinding.Read(ZSCompiler compiler)
            => new(1, [new IR.VM.GetGlobal(IR)], Type);

        RTType IRTBinding.Write(ZSCompiler compiler, IRTBinding value)
        {
            var code = value.Read(compiler);
            return new(
                code.StackSize + 1, 
                [.. code.Instructions, new IR.VM.Dup(), new IR.VM.SetGlobal(IR)], 
                Type
            );
        }
    }
}
