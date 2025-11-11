using ZSharp.Compiler;
using ZSharp.IR;

using TypeReferenceResult = IResult<ZSharp.IR.TypeReference, ZSharp.Compiler.Error>;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class GenericTypeInstance
        : ICompileIRType<TypeReference>
        , ICompileIRReference<TypeReference>
    {
        private TypeReference? IR { get; set; }

        TypeReferenceResult ICompileIRReference<TypeReference>.CompileIRReference(Compiler.Compiler compiler, object? target)
            => GetIR(compiler, target);

        TypeReferenceResult ICompileIRType<TypeReference>.CompileIRType(Compiler.Compiler compiler, object? target)
            => GetIR(compiler, target);

        private Result<TypeReference> GetIR(Compiler.Compiler compiler, object? target)
        {
            if (IR is null)
            {
                if (
                    compiler.IR.CompileDefinition<TypeDefinition>(Definition, target)
                    .When(out var definition)
                    .Error(out var error)
                ) return Result<TypeReference>.Error(error);

                var arguments = GenericArguments.Select(
                    arg => compiler.IR.CompileType(arg, target)
                ).ToArray();

                if (
                    arguments.Any(arg => arg.IsError)
                ) return Result<TypeReference>.Error(
                    arguments
                    .Where(arg => arg.IsError)
                    .Select(arg => arg.UnwrapError())
                );

                IR = new ConstructedTypeDefinition() {
                    Arguments = [ ..arguments.Select(arg => arg.Unwrap()) ],
                    Definition = definition!,
                };
            }

            return Result<TypeReference>.Ok(IR);
        }
    }
}
