namespace ZSharp.CTRuntime
{
    public sealed class Parameter(IR.Parameter parameter, ZSType type) 
        : IRBinding<IR.Parameter>(parameter, type)
        , IRTBinding
    {
        CTType IRTBinding.Type => Type;

        RTType IRTBinding.Read(ZSCompiler compiler)
            => new(1, [ new IR.VM.GetArgument(IR) ], Type);

        RTType IRTBinding.Write(ZSCompiler compiler, IRTBinding value)
        {
            var code = value.Read(compiler);
            if (!compiler.Interpreter.TypeSystem.IsAssignableTo(code.Type, Type))
                throw new Exception($"Cannot assign {code.Type} to {Type}");

            code.Instructions.Add(new IR.VM.Dup());
            code.Instructions.Add(new IR.VM.SetArgument(IR));
            return code;
        }
    }
}
