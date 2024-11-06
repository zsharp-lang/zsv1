using ZSharp.Compiler;

using Parameters = CommonZ.Utils.Collection<ZSharp.CGObjects.Parameter>;

namespace ZSharp.CGObjects
{
    public sealed class Signature : CGObject
    {
        public Parameters Args { get; init; } = [];

        public Parameter? VarArgs { get; set; }

        public Parameters KwArgs { get; init; } = [];

        public Parameter? VarKwArgs { get; set; }
    }
}
