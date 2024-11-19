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

        private Method Compile(RFunction node)
        {
            Method method = new(node.Name);

            objectBuilder.EnqueueForDependencyCollection(method, node);

            return method;
        }

        private Field Compile(RLetDefinition node)
        {
            Field field = new(node.Name ?? throw new());

            objectBuilder.EnqueueForDependencyCollection(field, node);

            return field;
        }

        private CGObject Compile(RVarDefinition node)
        {
            throw new NotImplementedException();
        }
    }
}
