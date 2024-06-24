namespace ZSharp.CTRuntime
{
    //internal sealed class CTFunctionType() : ZSValue(VM.TypeSystem.Type), ICallable
    //{
    //    public Code Call(ZSCompiler compiler, IBinding callable, Argument[] arguments)
    //    {
    //        var fn = compiler.EvaluateObject<VM.ZSFunction>(callable.Read());

    //        var args = arguments.Select(
    //            arg => compiler.EvaluateObject(arg.Binding.Read())
    //        ).ToArray();

    //        var result = compiler.Interpreter.Call(fn, args) ?? throw new Exception();

    //        if (VM.Interpreter.GetIR(result) is not IR.IRObject ir)
    //            throw new Exception();

    //        return new(1, [ new IR.VM.GetObject(ir) ], result.Type);
    //    }
    //}
}
