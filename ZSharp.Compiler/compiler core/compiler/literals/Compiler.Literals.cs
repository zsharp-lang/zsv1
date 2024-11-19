using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private CGObject trueObject = null!, falseObject = null!;

        private void InitializeLiterals()
        {
            trueObject = new TrueLiteral(TypeSystem.Boolean);
            falseObject = new FalseLiteral(TypeSystem.Boolean);
        }
    }
}
