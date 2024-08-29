using System.Diagnostics;
using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
    {
        public void Initialize(RTFunction function)
        {
            Declare(function);
            Define(function);

            //if (function.ReturnType is not null)
            //    AddDependenciesForDeclaration(function, function.ReturnType);

            //foreach (var arg in function.Signature.Args)
            //    AddDependencyForDeclaration(function, arg);

            //if (function.Signature.VarArgs is not null)
            //    AddDependencyForDeclaration(function, function.Signature.VarArgs);

            //foreach (var arg in function.Signature.KwArgs)
            //    AddDependencyForDeclaration(function, arg);

            //if (function.Signature.VarKwArgs is not null)
            //    AddDependencyForDeclaration(function, function.Signature.VarKwArgs);

            //if (function.Body is not null)
            //    AddDependenciesForDefinition(function, function.Body);
        }

        public void Initialize(Global global)
        {
            Declare(global);
            Define(global);
            //if (global.Type is not null)
            //{
            //    AddDependenciesForDeclaration(global, global.Type);
            //    if (global.Initializer is not null)
            //        AddDependenciesForDefinition(global, global.Initializer);
            //}
            //else if (global.Initializer is null)
            //    throw new Exception("Global variable must have a type or an initializer");
            //else
            //    AddDependenciesForDeclaration(global, global.Initializer);
        }
    }
}
