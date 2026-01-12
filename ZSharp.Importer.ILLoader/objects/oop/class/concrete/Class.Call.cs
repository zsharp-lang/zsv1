using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Class
        : ICTCallable
    {
        IResult ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (Constructor is null)
                return Result.Error("Class doesn't have a constructor and cannot be instantiated.");

            return compiler.CG.Call(Constructor, arguments);
        }
    }
}
