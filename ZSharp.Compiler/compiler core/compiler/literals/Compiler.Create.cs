using CommonZ.Utils;
using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private readonly Mapping<CompilerObject, NullLiteral> nullLiterals = [];

        public CompilerObject CreateTrue()
            => trueObject;

        public CompilerObject CreateFalse()
            => falseObject;

        public CompilerObject CreateFloat32(float value)
            => new Float32Literal(value, TypeSystem.Float32);

        public CompilerObject CreateInteger(DefaultIntegerType value)
            => new IntegerLiteral(value, TypeSystem.Int32); // TODO: fix type here

        public CompilerObject CreateString(string value)
            => new StringLiteral(value, TypeSystem.String);

        public CompilerObject CreateNull()
            => CreateNull(TypeSystem.Null);

        public CompilerObject CreateNull(CompilerObject type)
        {
            if (!nullLiterals.TryGetValue(type, out var nullLiteral))
                nullLiterals[type] = nullLiteral = new(type);

            return nullLiteral;
        }
    }
}
