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
                ROOPDefinition oop => Compile(oop),
                RVarDefinition var => Compile(var),
                _ => null,
            };

        private RTFunction Compile(RFunction node)
        {
            RTFunction function = new(node.Name);

            // TODO: make function overload group
            if (node.Name is not null && node.Name != string.Empty)
                Result.Members.Add(node.Name, Context.CurrentScope.Cache(node.Name, function));

            objectBuilder.EnqueueForDependencyCollection(function, node);

            return function;
        }

        private Global Compile(RLetDefinition node)
        {
            Global global = new(node.Name ?? throw new());

            Result.Members.Add(node.Name, Context.CurrentScope.Cache(node.Name, global));

            objectBuilder.EnqueueForDependencyCollection(global, node);

            return global;
        }

        private CGObject Compile(ROOPDefinition node)
            => node.Type switch
            {
                "class" => CompileClass(node),
                _ => throw new NotImplementedException(),
            };

        private Global Compile(RVarDefinition node)
        {
            Global global = new(node.Name ?? throw new());

            Result.Members.Add(node.Name, Context.CurrentScope.Cache(node.Name, global));

            objectBuilder.EnqueueForDependencyCollection(global, node);

            return global;
        }

        private Class CompileClass(ROOPDefinition node)
        {
            Class @class = new()
            {
                Name = node.Name,
            };

            if (node.Name is not null && node.Name != string.Empty)
                Result.Members.Add(node.Name, Context.CurrentScope.Cache(node.Name, @class));

            EnqueueForDependencyCollection(@class, node);

            return @class;
        }
    }
}
