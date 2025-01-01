//using ZSharp.Interop.IR2IL;
using ZSharp.Objects;
using ZSharp.Runtime.NET.IR2IL;

//using IL = System.Reflection.Emit;


namespace ZSharp.Runtime.NET
{
    internal sealed class IRCodeEvaluator(Runtime runtime) : Compiler.Evaluator
    {
        private readonly Runtime runtime = runtime;

        private Interpreter.Interpreter Interpreter => runtime.Interpreter;

        public override CompilerObject Evaluate(CompilerObject @object)
        {
            if (@object is not RawCode rawCode)
                return @object;

            var code = rawCode.Code;

            var module = new IR.Module("<CT>");

            var function = new IR.Function(Interpreter.Compiler.CompileIRType(code.RequireValueType()))
            {
                Name = "evaluate"
            };

            foreach (var m in code.Instructions
                .Select(i => 
                    i is IR.VM.IHasOperand hasOperand 
                    && hasOperand.Operand is IR.IRObject ir
                    ? ir.Module : null)
                .Where(i => i is not null)
            )
                Interpreter.Runtime.Import(m!);

            function.Body.Instructions.AddRange([
                .. code.Instructions,
                new IR.VM.Return()
            ]);

            module.Functions.Add(function);

            var functionIL = runtime.Import(module).GetType("<Globals>")!.GetMethod(function.Name)!;
            var value = functionIL.Invoke(null, null);

            if (value is CompilerObject co)
                return co;

            if (value is ICompileTime coObject)
                return coObject.GetCO();

            //if (value is Type type)
            //    return new RawType(interpreter.IRInterop.ImportILType(type), interpreter.Compiler.TypeSystem.Type);

            throw new NotImplementedException();
        }

        private void CompileGetObject(ICodeLoader loader, IR.VM.GetObject get)
        {

            if (get.IR is IR.IType type)
            {
                loader.Output.Emit(IL.Emit.OpCodes.Ldtoken, runtime.Import(type));
                //loader.Output.Emit()
                loader.Push(typeof(Type));
            }

            else throw new NotImplementedException();
        }
    }
}
