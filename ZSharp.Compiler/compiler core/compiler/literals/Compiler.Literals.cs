using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private CompilerObject trueObject = null!, falseObject = null!;

        private void InitializeLiterals()
        {
            trueObject = new TrueLiteral(TypeSystem.Boolean);
            falseObject = new FalseLiteral(TypeSystem.Boolean);
        }
    }
}
