using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ClassSpecCompiler
    {
        public void Initialize(ModuleOOPObject oop, ROOPDefinition node)
        {
            objectBuilder.AddDependenciesForDeclaration();

            objectBuilder.AddDependencyForDefinition(oop);

            if (node.Bases is not null)
                foreach (var item in node.Bases)
                    objectBuilder.AddDependenciesForDeclaration(item);

            if (node.Content is not null)
                objectBuilder.AddDependenciesForDefinition(node.Content);

            // TODO: also parameters (primary constructor) and class parameters.\

            // TODO: support metaclass
        }

        public void Initialize(Field field, RLetDefinition node)
        {
            objectBuilder.AddDependenciesForDeclaration();

            objectBuilder.AddDependencyForDefinition(field);

            if (node.Type is not null)
            {
                objectBuilder.AddDependenciesForDeclaration(node.Type);
                objectBuilder.AddDependenciesForDefinition(node.Value);
            }
            else
                objectBuilder.AddDependenciesForDeclaration(node.Value);
        }

        //public void Initialize(Global global, RVarDefinition node)
        //{
        //    objectBuilder.AddDependenciesForDeclaration();

        //    objectBuilder.AddDependencyForDefinition(global);

        //    if (node.Type is not null)
        //    {
        //        objectBuilder.AddDependenciesForDeclaration(node.Type);
        //        if (node.Value is not null)
        //            objectBuilder.AddDependenciesForDefinition(node.Value);
        //    }
        //    else if (node.Value is null)
        //        throw new Exception("Global variable must have a type or an initializer");
        //    else
        //        objectBuilder.AddDependenciesForDeclaration(node.Value);
        //}

        public void Initialize(Method method, RFunction node)
        {
            objectBuilder.AddDependenciesForDeclaration();

            objectBuilder.AddDependencyForDefinition(method);

            Parameter InitializeParameter(RParameter node)
            {
                Parameter parameter = new(node.Name!);

                if (node.Type is not null)
                {
                    objectBuilder.AddDependenciesForDeclaration(node.Type);
                    if (node.Default is not null)
                        objectBuilder.AddDependenciesForDefinition(node.Default);
                }
                else if (node.Default is null)
                    throw new Exception("Parameter must have a type or an initializer");
                else
                    objectBuilder.AddDependenciesForDeclaration(node.Default);

                objectBuilder.Nodes.Cache(parameter, new NodeObject(node, parameter));

                return parameter;
            }

            if (node.ReturnType is not null)
                objectBuilder.AddDependenciesForDeclaration(node.ReturnType);

            if ((node.Signature.Args?.Count ?? 0) != 1)
                throw new();

            if (node.Signature.Args![0].Type is null)
                node.Signature.Args[0].Type = Node.Id;

            foreach (var arg in node.Signature.Args ?? [])
                method.Signature.Args.Add(InitializeParameter(arg));

            if (node.Signature.VarArgs is not null)
                method.Signature.VarArgs = InitializeParameter(node.Signature.VarArgs);

            foreach (var arg in node.Signature.KwArgs ?? [])
                method.Signature.KwArgs.Add(InitializeParameter(arg));

            if (node.Signature.VarKwArgs is not null)
                method.Signature.VarKwArgs = InitializeParameter(node.Signature.VarKwArgs);

            if (node.Body is not null)
                objectBuilder.AddDependenciesForDefinition(node.Body);
        }
    }
}
