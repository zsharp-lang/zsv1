using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class DocumentCompiler
    {
        private CGObject Compile(RStatement statement)
            => statement switch
            {
                _ => Compiler.CompileNode(statement),
            };

        #region RDefinition

        private CGObject Compile(RModule module)
        {
            var result = Compiler.CompileNode<RModule, Module>(module);

            Result.IR.Submodules.Add(result.IR!);

            return result;
        }

        #endregion
    }
}
