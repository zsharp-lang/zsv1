using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
    {
        public void Initialize(RTFunction function)
        {
            AddDependencyForDefinition(function);

            //Declare(function);
            //Define(function);

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
                AddDependenciesForDefinition(function.Body);
        }

        public void Initialize(Global global)
        {
            AddDependencyForDefinition(global);

            //Declare(global);
            //Define(global);
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
