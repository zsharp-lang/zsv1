using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
    {
        private void Declare(RTFunction function)
        {
            IR.Parameter Declare(Parameter parameter)
            {
                var type = 
                    parameter.Type is null ? null :
                    IRGenerator.EvaluateType(parameter.Type);

                var initializerCode = 
                    parameter.Initializer is null ? null :
                    IRGenerator.Read(IRGenerator.CG.Run(parameter.Initializer));

                IRGenerator.CG.Context.Set(parameter.Name, parameter);

                return parameter.IR = new(
                    parameter.Name, 
                    type ?? initializerCode?.RequireValueType() ?? throw new Exception())
                {
                    Initializer = initializerCode?.Instructions?.ToArray()
                };
            }

            using (IRGenerator.ContextOf(function)) // TODO: remove this when module becomes dependency aware
            {
                IRType ? returnType = null;

                if (function.ReturnType is not null)
                    returnType = IRGenerator.EvaluateType(function.ReturnType);

                if (returnType is null)
                    throw new NotImplementedException("Implicit return type");

                function.IR = new(returnType)
                {
                    Name = function.Name,
                };

                Object.IR!.Functions.Add(function.IR);

                foreach (var arg in function.Signature.Args)
                    function.IR.Signature.Args.Parameters.Add(Declare(arg));

                if (function.Signature.VarArgs is not null)
                    function.IR.Signature.Args.Var = Declare(function.Signature.VarArgs);

                foreach (var arg in function.Signature.KwArgs)
                    function.IR.Signature.KwArgs.Parameters.Add(Declare(arg));

                if (function.Signature.VarKwArgs is not null)
                    function.IR.Signature.KwArgs.Var = Declare(function.Signature.VarKwArgs);
            }
        }

        private void Declare(Global global)
        {
            var declaredType = global.Type is null ? null : IRGenerator.EvaluateType(global.Type);

            var initializerCode = global.Initializer is null
                ? null : IRGenerator.Read(IRGenerator.CG.Run(global.Initializer));

            if (initializerCode is not null)
            {
                var initializerType = initializerCode.RequireValueType();

                declaredType ??= initializerType;

                var assignmentCode = IRGenerator.AssignTo(initializerType, declaredType);

                if (assignmentCode is not null)
                    initializerCode.Append(assignmentCode);
            }
            else if (declaredType is null) throw new Exception("Unknown type!");

            if (initializerCode is null)
                global.IR = new IR.Global(global.Name, declaredType);
            else
            {
                global.IR = new IR.Global(global.Name, declaredType)
                {
                    Initializer = [.. initializerCode.Instructions]
                    // todo: ^^^ should the IR also contain information about stack size?
                };

                var initIR = Object.InitFunction.IR!;
                initIR.Body.StackSize = Math.Max(initIR.Body.StackSize, initializerCode.MaxStackSize);
                initIR.Body.Instructions.AddRange([
                    ..initializerCode.Instructions,
                new IR.VM.SetGlobal(global.IR)
                ]);
            }
            
            Object.IR!.Globals.Add(global.IR);
        }
    }
}
