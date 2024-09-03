using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
    {
        public void Initialize(RTFunction function)
        {
            AddDependencyForDefinition(function);

            void Initialize(Parameter parameter)
            {
                if (parameter.Type is not null)
                {
                    AddDependenciesForDeclaration(parameter.Type);
                    if (parameter.Initializer is not null)
                        AddDependenciesForDefinition(parameter.Initializer);
                }
                else if (parameter.Initializer is null)
                    throw new Exception("Parameter must have a type or an initializer");
                else
                    AddDependenciesForDeclaration(parameter.Initializer);
            }

            void InitializeBody(CGCode body)
            {
                foreach (var instruction in body)
                {
                    CGObject? @object = instruction switch
                    {
                        CGRuntime.HLVM.Get getInstruction =>
                            IRGenerator.CurrentScope.Cache(getInstruction.Name),
                        CGRuntime.HLVM.Object @objectInstruction =>
                            objectInstruction.CGObject,
                        _ => null
                    };

                    if (@object is null) continue;

                    if (@object is Local local)
                    {
                        if (local.Initializer is not null)
                            AddDependenciesForDefinition(local.Initializer);
                        if (local.Type is not null)
                            AddDependenciesForDefinition(local.Type);
                    }

                    AddDependenciesForDefinition(DependencyState.Declared, @object);
                }
            }

            if (function.ReturnType is not null)
                AddDependenciesForDeclaration(function.ReturnType);

            foreach (var arg in function.Signature.Args)
                Initialize(arg);

            if (function.Signature.VarArgs is not null)
                Initialize(function.Signature.VarArgs);

            foreach (var arg in function.Signature.KwArgs)
                Initialize(arg);

            if (function.Signature.VarKwArgs is not null)
                Initialize(function.Signature.VarKwArgs);

            if (function.Body is not null)
                InitializeBody(function.Body);
        }

        public void Initialize(Global global)
        {
            AddDependencyForDefinition(global);

            if (global.Type is not null)
            {
                AddDependenciesForDeclaration(global.Type);
                if (global.Initializer is not null)
                    AddDependenciesForDefinition(global.Initializer);
            }
            else if (global.Initializer is null)
                throw new Exception("Global variable must have a type or an initializer");
            else
                AddDependenciesForDeclaration(global.Initializer);
        }
    }
}
