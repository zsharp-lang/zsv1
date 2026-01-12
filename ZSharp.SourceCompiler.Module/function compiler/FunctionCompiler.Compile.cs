namespace ZSharp.SourceCompiler.Module
{
    partial class FunctionCompiler
    {
        public IResult Declare()
        {
            Error? error = null;

            if (Object.Name != string.Empty)
                if (!Interpreter
                    .Compiler
                    .CurrentContext
                    .PerformOperation<IScopeContext>(
                        scope => !scope.Add(Object.Name, Object).Error(out error)
                    )
                )
                    return Result.Error($"Could not bind function {Node.Name}: {error}");

            return Result.Ok(Object);
        }

        public void CompileSignature()
        {
            if (Node.ReturnType is null)
                Interpreter.Log.Error($"Function {Node.Name} must have a return type.", Node);
            else if (
                CompileExpression(Node.ReturnType)
                .When(out var returnType)
                .Error(out var error)
            ) Interpreter.Log.Error($"{error}", Node.ReturnType);
            else if (
                Interpreter.Compiler.CG.Get(returnType!)
                .When(out returnType)
                .Error(out error)
            ) Interpreter.Log.Error($"{error}", Node.ReturnType);
            else if (
                Interpreter.Compiler.Evaluator.Evaluate(returnType!)
                .When(out returnType)
                .Error(out error)
            ) Interpreter.Log.Error($"{error}", Node.ReturnType);
            else Object.ReturnType = returnType;

            if (Node.Signature.VarArgs is not null)
                Interpreter.Log.Error("VarArgs functions are obsolete and should not be used.", Node.Signature.VarArgs);
            if (Node.Signature.VarKwArgs is not null)
                Interpreter.Log.Error("VarKwArgs functions are obsolete and should not be used.", Node.Signature.VarKwArgs);

            foreach (var param in Node.Signature.Args ?? [])
                if (Compile(param).When(out var co).Error(out var error))
                    Interpreter.Log.Error(error.ToString()!, param);
            //else result.Signature.Args.Add(co!);

            foreach (var param in Node.Signature.KwArgs ?? [])
                if (Compile(param).When(out var co).Error(out var error))
                    Interpreter.Log.Error(error.ToString()!, param);
            //else result.Signature.KwArgs.Add(co!);
        }

        public void CompileBody()
        {
            if (Node.Body is null)
                return;

            if (
                new FunctionBodyCompiler(Interpreter, Node.Body).Compile()
                .When(out var body)
                .Error(out var error)
            )
                Interpreter.Log.Error(error.ToString()!, Node.Body);
            else
                Object.Body = body;
        }

        private IResult Compile(AST.Parameter parameter)
        {
            return Result.Error($"Parameter {parameter.Name} did not compile because parameter compilation is not implemented.");
        }
    }
}
