namespace ZSharp.Platform.Runtime.Loaders
{
    partial class CodeCompiler_Impl
    {
        public static void Compile(ITransformableCodeContext ctx, TransformCall instruction)
        {
            var transformer = ctx.GetTransformer(instruction) ?? throw new($"No transformer found for instruction: {instruction}");
            transformer(ctx, instruction);
        }
    }
}
