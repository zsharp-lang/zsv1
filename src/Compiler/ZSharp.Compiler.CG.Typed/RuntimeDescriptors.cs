using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public static class RuntimeDescriptors
    {
        public static IResult Get(TS ts, CompilerObject @object)
        {
            if (@object.Is<IHasRuntimeDescriptor>(out var hasRuntimeDescriptor))
                return hasRuntimeDescriptor.GetRuntimeDescriptor();

            if (ts.IsTyped(@object, out var type))
                return Result.Ok(type);

            return Result.Error($"No runtime descriptor found for object {@object}.");
        }

        public static bool Find(TS ts, CompilerObject @object, [NotNullWhen(true)] out CompilerObject? descriptor)
            => Get(ts, @object).Ok(out descriptor);
    }
}
