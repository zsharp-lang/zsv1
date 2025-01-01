using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public bool IsCTValue(CompilerObject @object)
        {
            if (IsLiteral(@object)) return true;

            return false; // TODO: implement the CTValue protocol
        }

        public bool IsCTValue<T>(CompilerObject @object, [NotNullWhen(true)] out T? value)
            where T : class
        {
            if (IsLiteral(@object, out value)) return true;

            return false; // TODO: implement CTValue protocol
        }

        public bool IsCTValue<T>(CompilerObject @object, [NotNullWhen(true)] out T? value)
            where T : struct
        {
            if (IsLiteral(@object, out value)) return true;

            return false; // TODO: implement CTValue protocol
        }
    }
}
