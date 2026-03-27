using ZSharp.Compiler;

namespace Core.LanguageExtensions.Objects
{
    partial class Class
        : ICTCallable
    {
        IResult ICTCallable.Call(Compiler compiler, Argument[] arguments)
        {
            if (Constructor is null)
                return Result.Error("Class does not have a constructor.");

            return compiler.CG.Call(Constructor, arguments);
        }
    }
}
