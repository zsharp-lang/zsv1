using System.Net.WebSockets;
using ZSharp.Compiler;
using ZSharp.Compiler.Features.Callable;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class Parameter
        : CompilerObject
        , IParameter
    {
        public string Name { get; }

        public CompilerObject Type { get; }

        public Parameter(IL.ParameterInfo il, ILLoader loader)
        {
            IL = il;

            Name = IL.AliasOrName();
            Type = loader.LoadType(IL.ParameterType);
        }

        Result IParameter.Match(Compiler.Compiler compiler, IArgumentStream arguments)
        {
            if (!arguments.PopArgument(Name, out var argument) &&
                !arguments.PopArgument(out argument)
            )
                if (IL.HasDefaultValue) return Result.Error("Default values are not supported yet.");
                else return Result.Error($"No argument provided for parameter '{this}'.");

            return compiler.CG.ImplicitCast(argument, Type);
        }
    }
}
