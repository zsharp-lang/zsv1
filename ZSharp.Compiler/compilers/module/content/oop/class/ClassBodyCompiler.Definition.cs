using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ClassSpecCompiler
    {
        protected override CGObject? CompileContextItem(RDefinition definition)
            => definition switch
            {
                RFunction function => Compile(function),
                RLetDefinition let => Compile(let),
                RVarDefinition var => Compile(var),
                _ => null,
            };

        private RTFunction Compile(RFunction node)
        {
            throw new NotImplementedException();
        }

        private Field Compile(RLetDefinition node)
        {
            Field field = new(node.Name ?? throw new());

            Result.Content!.Add(field);

            objectBuilder.EnqueueForDependencyCollection(field, node);

            return field;
        }

        private CGObject Compile(RVarDefinition node)
        {
            throw new NotImplementedException();
        }
    }
}
