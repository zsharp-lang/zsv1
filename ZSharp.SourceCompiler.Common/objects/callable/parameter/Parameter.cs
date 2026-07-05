using ZSharp.Compiler.Features.Callable;

namespace ZSharp.SourceCompiler.Objects
{
    public sealed partial class Parameter
        : CompilerObject
        , IParameter
    {
        public required string Name { get; init; }

        public required CompilerObject Type { get; init; }

        public CompilerObject? DefaultValue { get; set; }

        IResult IParameter.Match(ZSharp.Compiler.Compiler compiler, IArgumentStream arguments)
        {
            if (!arguments.PopArgument(Name, out var argument) &&
                !arguments.PopArgument(out argument)
            )
                if (DefaultValue is not null) return Result.Error("Default values are not supported yet.");
                else return Result.Error($"No argument provided for parameter '{this}'.");

            return compiler.CG.ImplicitCast(argument, Type);
        }
    }
}
