using ZSharp.Compiler.Features.Callable;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed partial class ThisParameter
        : CompilerObject
        , IParameter
    {
        public string Name => "this";

        public CompilerObject Type { get; }

        public ThisParameter(IL.MethodBase il, ILLoader loader)
        {
            IL = il;

            Type = loader.LoadType(IL.DeclaringType ?? throw new());
        }

        IResult IParameter.Match(Compiler.Compiler compiler, IArgumentStream arguments)
        {
            if (!arguments.PopArgument(Name, out var argument) &&
                !arguments.PopArgument(out argument)
            )
                return Result.Error($"No argument provided for parameter '{Name}'.");

            return compiler.CG.ImplicitCast(argument, Type);
        }
    }
}
