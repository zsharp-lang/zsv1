using CommonZ.Utils;

namespace ZSharp.Objects
{
    public sealed class ReferenceContext
    {
        public Mapping<CompilerObject, CompilerObject> CompileTimeValues { get; set; } = [];

        public static ReferenceContext Empty { get; } = new()
        {
            CompileTimeValues = []
        };
    }
}
