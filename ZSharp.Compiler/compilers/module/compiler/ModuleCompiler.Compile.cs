using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleCompiler
    {
        protected override CGObject? CompileContextItem(RDefinition definition)
            => definition switch
            {
                RFunction function => Compile(function),
                RLetDefinition let => Compile(let),
                _ => null,
            };

        private CGObject Compile(RFunction node)
        {
            RTFunction function = new(node.Name);

            // TODO: make function overload group
            if (node.Name is not null && node.Name != string.Empty)
                Result.Members.Add(node.Name, Context.CurrentScope.Cache(node.Name, function));

            EnqueueForDependencyCollection(function, node);

            return function;
        }

        private CGObject Compile(RLetDefinition node)
        {
            Global global = new(node.Name ?? throw new());

            Result.Members.Add(node.Name, Context.CurrentScope.Cache(node.Name, global));

            EnqueueForDependencyCollection(global, node);

            return global;
        }
    }
}
