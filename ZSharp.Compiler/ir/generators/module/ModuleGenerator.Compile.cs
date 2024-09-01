using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
    {
        private void Compile(CGObject @object)
        {
            switch (@object)
            {
                case RTFunction function: Compile(function); break;
                case Global global: Compile(global); break;
                case Module module: Compile(module); break;
                default: throw new NotImplementedException();
            }
        }

        private void Compile(RTFunction function)
        {
            // TODO: Implement function overloads.
            //if (function.Name is not null && function.Name != string.Empty)
            //{
            //    if (!IRGenerator.CurrentScope.Cache<FunctionOverloadGroup>(function.Name, out var functionGroup))
            //        functionGroup = IRGenerator.CurrentScope.Cache(function.Name, new FunctionOverloadGroup(function.Name));

            //    if (functionGroup is null)
            //        throw new Exception($"Name {function.Name} is already used for a different kind of object.");

            //    functionGroup.Overloads.Add(function);

            //    // if the group is constructed for the first time, we might want to add it
            //    // to the dependency queue. The group will depend on all its overload declarations
            //    // for declarations so that it can build an optimized overload tree.
            //    // However, for now, we will only add the functions themselves.
            //}

            EnqueueForDependencyCollection(function);
            //Initialize(function);
        }

        private void Compile(Global global)
            => EnqueueForDependencyCollection(global);
            //=> Initialize(global);

        private void Compile(Module module)
            => Object.IR!.Submodules.Add(new ModuleGenerator(IRGenerator, module).Run());
    }
}
