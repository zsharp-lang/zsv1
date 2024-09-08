using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleCompiler
    {
        public void Initialize(Class @class, ROOPDefinition node)
        {
            AddDependenciesForDeclaration();

            AddDependencyForDefinition(@class);

            if (node.Bases is not null)
                foreach (var item in node.Bases)
                    AddDependenciesForDeclaration(item);

            if (node.Content is not null)
                AddDependenciesForDefinition(node.Content);

            // TODO: also parameters (primary constructor) and class parameters.\

            // TODO: support metaclass
        }

        public void Initialize(Global global, RLetDefinition node)
        {
            AddDependenciesForDeclaration();

            AddDependencyForDefinition(global);

            if (node.Type is not null)
            {
                AddDependenciesForDeclaration(node.Type);
                AddDependenciesForDefinition(node.Value);
            }
            else
                AddDependenciesForDeclaration(node.Value);
        }

        public void Initialize(Global global, RVarDefinition node)
        {
            AddDependenciesForDeclaration();

            AddDependencyForDefinition(global);

            if (node.Type is not null)
            {
                AddDependenciesForDeclaration(node.Type);
                if (node.Value is not null)
                    AddDependenciesForDefinition(node.Value);
            }
            else if (node.Value is null)
                throw new Exception("Global variable must have a type or an initializer");
            else
                AddDependenciesForDeclaration(node.Value);
        }

        public void Initialize(RTFunction function, RFunction node)
        {
            AddDependenciesForDeclaration();

            AddDependencyForDefinition(function);

            Parameter InitializeParameter(RParameter node)
            {
                Parameter parameter = new(node.Name!);

                if (node.Type is not null)
                {
                    AddDependenciesForDeclaration(node.Type);
                    if (node.Default is not null)
                        AddDependenciesForDefinition(node.Default);
                }
                else if (node.Default is null)
                    throw new Exception("Parameter must have a type or an initializer");
                else
                    AddDependenciesForDeclaration(node.Default);

                nodes.Cache(parameter, new NodeObject(node, parameter));

                return parameter;
            }

            if (node.ReturnType is not null)
                AddDependenciesForDeclaration(node.ReturnType);

            foreach (var arg in node.Signature.Args ?? [])
                function.Signature.Args.Add(InitializeParameter(arg));

            if (node.Signature.VarArgs is not null)
                function.Signature.VarArgs = InitializeParameter(node.Signature.VarArgs);

            foreach (var arg in node.Signature.KwArgs ?? [])
                function.Signature.KwArgs.Add(InitializeParameter(arg));

            if (node.Signature.VarKwArgs is not null)
                function.Signature.VarKwArgs = InitializeParameter(node.Signature.VarKwArgs);

            if (node.Body is not null)
                AddDependenciesForDefinition(node.Body);
        }
    }
}
