using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
        : ICompiler
    {
        private CGObject trueObject = null!, falseObject = null!;

        private void InitializeLiterals()
        {
            trueObject = new TrueLiteral(RuntimeModule.TypeSystem.Boolean);
            falseObject = new FalseLiteral(RuntimeModule.TypeSystem.Boolean);
        }
    }
}
