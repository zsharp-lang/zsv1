using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleCompiler
    {
        private void Declare(ModuleOOPObject oop, ROOPDefinition node)
        {
            oop.Object = Compiler.CreateClass(oop.Spec.MetaClass, oop.Spec.Name);

            if (node.Name is not null && node.Name != string.Empty)
                if (Context.CurrentScope.Uncache(node.Name))
                    Context.CurrentScope.Cache(node.Name, oop.Object);
        }

        private void Declare(Global global, RLetDefinition node)
        {
            global.Initializer = Compiler.CompileNode(node.Value);

            if (node.Type is not null)
                global.Type = Compiler.CompileNode(node.Type);

            var type = global.Type is null ? null : Compiler.CompileIRType(global.Type);
            var initializer = Compiler.CompileIRCode(global.Initializer);

            type ??= initializer.RequireValueType();

            global.IR = new IR.Global(global.Name, type)
            {
                Initializer = [.. initializer.Instructions]
            };
            
            Result.IR!.Globals.Add(global.IR);
        }

        private void Declare(Global global, RVarDefinition node)
        {
            if (node.Value is not null)
                global.Initializer = Compiler.CompileNode(node);

            if (node.Type is not null)
                global.Type = Compiler.CompileNode(node.Type);

            var type = global.Type is null 
                ? null 
                : Compiler.CompileIRType(global.Type);
            var initializer = global.Initializer is null 
                ? null 
                : Compiler.CompileIRCode(global.Initializer);

            type ??= initializer?.RequireValueType();

            if (type is null)
                throw new();

            global.IR = new IR.Global(global.Name, type)
            {
                Initializer = initializer is null ? null : [.. initializer.Instructions]
            };

            Result.IR!.Globals.Add(global.IR);
        }

        private void Declare(RTFunction function, RFunction node)
        {
            IR.Parameter Declare(Parameter parameter)
            {
                var node = (RParameter)objectBuilder.Nodes.Cache(parameter)!.Node;

                parameter.Type =
                    node.Type is null ? null :
                    Compiler.CompileNode(node.Type);

                parameter.Initializer =
                    node.Default is null ? null :
                    Compiler.CompileNode(node.Default);

                var initializer = parameter.Initializer is null
                    ? null
                    : Compiler.CompileIRCode(parameter.Initializer);
                var type = parameter.Type is null
                    ? null
                    : Compiler.CompileIRType(parameter.Type);

                if (type is null && initializer is null)
                    throw new();
                else type ??= initializer!.RequireValueType();

                Context.CurrentScope.Cache(parameter.Name, parameter);

                return parameter.IR = new(parameter.Name, type)
                {
                    Initializer = initializer?.Instructions.ToArray()
                };
            }

            using (Context.For(function))
            {

                if (node.ReturnType is not null)
                    function.ReturnType = Compiler.CompileNode(node.ReturnType);

                IRType ? returnType = null;

                if (function.ReturnType is not null)
                    returnType = Compiler.CompileIRType(function.ReturnType);

                if (returnType is null)
                    throw new NotImplementedException("Implicit return type");

                function.IR = new(returnType)
                {
                    Name = function.Name,
                };

                Result.IR!.Functions.Add(function.IR);

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

    }
}
