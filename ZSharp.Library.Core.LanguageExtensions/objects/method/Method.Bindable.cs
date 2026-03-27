using ZSharp.Compiler;
using ZSharp.Compiler.Features.OOP;
using ZSharp.Importer.ILLoader.Objects;

namespace Core.LanguageExtensions.Objects
{
    partial class Method
        : IBindable
    {
        IResult<CompilerObject, Error> IBindable.Bind(Compiler compiler, CompilerObject @object)
        {
            return Result.Ok(new BoundMethod()
            {
                Method = this,
                Object = @object
            });
        }
    }
}
