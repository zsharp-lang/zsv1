using ZSharp.Compiler;
using ZSharp.Compiler.Features;
using ZSharp.SourceCompiler;

namespace Core.Runtime.Objects
{
    partial class Class
        : IRTImplicitCastTo
    {
        IResult IRTImplicitCastTo.ImplicitCast(Compiler compiler, CompilerObject @object, CompilerObject type)
        {
            CompilerObject? @base = this;
            do
            {
                if (compiler.Reflection.IsSameDefinition(@base, type))
                    return Result.Ok(@object);
            } while ((@base = @base!.As<ISingleInheritance>()?.Base) is not null);

            return Result.Error($"Cannot implicitly cast '{@object}' to type '{type}'.");
        }
    }
}
