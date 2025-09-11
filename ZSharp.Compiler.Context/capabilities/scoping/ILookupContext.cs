using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public interface ILookupContext
        : IContext
    {
        public Result Get(MemberName name);

        public sealed bool Get(MemberName name, [NotNullWhen(true)] out CompilerObject? value)
            => Get(name).Ok(out value);
    }
}
