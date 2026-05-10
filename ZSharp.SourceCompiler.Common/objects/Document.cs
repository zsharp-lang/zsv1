
namespace ZSharp.SourceCompiler
{
    public sealed class Document
        : CompilerObject
        , ICompileIRCode
    {
        internal List<CompilerObject> Content { get; } = [];

        IResult<IRCode, Error> ICompileIRCode.CompileIRCode(ZSharp.Compiler.Compiler compiler, object? target)
        {
            var results = Content.Select(item => compiler.IR.CompileCode(item, target));

            if (results.Any(result => result.IsError))
                return CommonZ.Result<IRCode, Error>.Error(
                    new AggregateError(
                        results
                        .Where(result => result.IsError)
                        .Select(result => result.UnwrapError())
                    )
                );

            var code = new IRCode();
            foreach (var result in results) code.Append(result.Unwrap());

            return CommonZ.Result<IRCode, Error>.Ok(code);
        }
    }
}
