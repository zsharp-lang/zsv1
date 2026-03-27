using ZSharp.Compiler;

namespace Core.LanguageExtensions.Objects
{
    partial class OverloadGroup
        : ICOProxy
    {
        IResult ICOProxy.Apply(Func<CompilerObject, IResult> fn)
        {
            OverloadGroup resultGroup = new() { Name = Name };

            foreach (var overload in overloads)
                if (
                    fn(overload)
                    .Ok(out var item)
                ) resultGroup.AddOverload(item);

            if (resultGroup.overloads.Count == 0)
                return Result.Error("No overloads could be proxied.");

            return Result.Ok(resultGroup);
        }
    }
}
