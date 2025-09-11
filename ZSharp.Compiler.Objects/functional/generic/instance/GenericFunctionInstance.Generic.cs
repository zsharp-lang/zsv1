using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    partial class GenericFunctionInstance
    {
        public required Mapping<GenericParameter, IType> GenericArguments { get; init; }

        public required GenericFunction GenericFunction { get; init; }
    }
}
