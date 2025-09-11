using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public static class CompilerExtensions
    {
        public static Result GetRuntimeDescriptor(this Compiler compiler, CompilerObject @object)
        {
            if (@object.Is<IHasRuntimeDescriptor>(out var hasRuntimeDescriptor))
                return hasRuntimeDescriptor.GetRuntimeDescriptor(compiler);

            return Result.Error($"No runtime descriptor found for object {@object}.");
        }

        public static bool RuntimeDescriptor(this Compiler compiler, CompilerObject @object, [NotNullWhen(true)] out CompilerObject? rt)
            => compiler.GetRuntimeDescriptor(@object).Ok(out rt);
    }
}
