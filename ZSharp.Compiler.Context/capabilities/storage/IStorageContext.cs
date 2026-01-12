using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public interface IStorageContext
    {
        public IResult CreateStorage(StorageOptions options);

        public sealed bool CreateStorage(StorageOptions options, [NotNullWhen(true)] out CompilerObject? result)
            => CreateStorage(options).Ok(out result);
    }
}
