using System.Diagnostics.CodeAnalysis;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ZSSourceCompiler
    {
        public bool UnpackResult<T>(
            Compiler.Result<T, string> result,
            [NotNullWhen(true)] out T? value,
            Node origin
        )
            where T : class
        {
            if (result.Error(out var error)) {
                value = null;
                LogError(error, origin);
                return false;
            }
            value = result.Unwrap();
            return true;
        }
    }
}
