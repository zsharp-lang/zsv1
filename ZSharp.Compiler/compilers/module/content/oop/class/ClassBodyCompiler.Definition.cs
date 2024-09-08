using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ClassBodyCompiler
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

        private Global Compile(RLetDefinition node)
        {
            throw new NotImplementedException();
        }

        private CGObject Compile(RVarDefinition node)
        {
            throw new NotImplementedException();
        }
    }
}
