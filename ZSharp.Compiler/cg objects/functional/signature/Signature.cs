using ZSharp.Compiler;

using Parameters = CommonZ.Utils.Collection<ZSharp.Objects.Parameter>;

namespace ZSharp.Objects
{
    public sealed class Signature : CompilerObject
    {
        public Parameters Args { get; init; } = [];

        public Parameter? VarArgs { get; set; }

        public Parameters KwArgs { get; init; } = [];

        public Parameter? VarKwArgs { get; set; }
    }
}
