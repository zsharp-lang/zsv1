using CommonZ.Utils;
using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private Parameter LoadIR(IR.Parameter parameter)
            => new(parameter.Name)
            {
                Initializer = parameter.Initializer is null ? null : new RawCode(new(parameter.Initializer)
                {
                    MaxStackSize = 0, // TODO
                    Types = [ImportIR(parameter.Type)]
                }),
                IR = parameter,
                Type = new RawType(parameter.Type, TypeSystem.Type)
            };

        private RTFunction LoadIR(IR.Function function)
        {
            RTFunction result = new(function.Name)
            {
                IR = function,
                ReturnType = new RawType(function.ReturnType, TypeSystem.Type)
            };

            if (function.Signature.HasArgs)
                foreach (var arg in function.Signature.Args.Parameters)
                    result.Signature.Args.Add(LoadIR(arg));

            if (function.Signature.IsVarArgs)
                result.Signature.VarArgs = LoadIR(function.Signature.Args.Var!);

            if (function.Signature.HasKwArgs)
                foreach (var kwArg in function.Signature.KwArgs.Parameters)
                    result.Signature.KwArgs.Add(LoadIR(kwArg));

            if (function.Signature.IsVarKwArgs)
                result.Signature.VarKwArgs = LoadIR(function.Signature.KwArgs.Var!);

            return result;
        }

        private Module LoadIR(IR.Module module)
        {
            Module result = new(module.Name!)
            {
                IR = module,
            };

            if (module.HasSubmodules)
                foreach (var submodule in module.Submodules)
                    result.Members.Add(submodule.Name ?? throw new(), ImportIR(submodule));

            if (module.HasGlobals)
                foreach (var global in module.Globals)
                    result.Members.Add(global.Name, ImportIR(global));

            if (module.HasFunctions)
                foreach (var function in module.Functions)
                    if (function.Name is not null && function.Name != string.Empty)
                    {
                        if (!result.Members.TryGetValue(function.Name, out var cg) || cg is not SimpleFunctionOverloadGroup group)
                            result.Members[function.Name] = group = new(function.Name);
                        group.Overloads.Add(ImportIR(function));
                    }
                        
                        

            return result;
        }

        private Class LoadIR(IR.Class @class)
        {
            Class result = new()
            {
                Base = @class.Base is null ? null : ImportIR(@class.Base),
                IR = @class,
                Name = @class.Name
            };

            //if (@class.HasFields)
            //    foreach (var field in @class.Fields)
            //        result.Members.Add(field.Name, ImportIR(field));

            return result;
        }
    }
}
