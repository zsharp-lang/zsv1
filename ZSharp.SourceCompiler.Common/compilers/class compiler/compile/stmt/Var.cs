namespace ZSharp.SourceCompiler.Class
{
    partial class ClassCompiler
    {
        //private IResult Compile(AST.VarStatement var)
        //{
        //    var result = new Objects.Local()
        //    {
        //        Name = var.Name
        //    };

        //    tasks.AddTask(() =>
        //    {
        //        if (var.Value is not null)
        //            if (
        //                Compile(var.Value)
        //                .When(out var value)
        //                .Error(out var error)
        //            )
        //                Error($"Could not compile value for local '{var.Name}': {error}", var.Value);
        //            else result.Value = value;

        //        if (var.Type is not null)
        //        {
        //            if (
        //                Compile(var.Type)
        //                .When(out var type)
        //                .Error(out var error)
        //            )
        //                Error($"Could not compile type for local '{var.Name}': {error}", var.Type);
        //            else result.Type = type;
        //        }
        //        else if (
        //            result.Value is not null
        //        )
        //            if (
        //                Interpreter.Compiler.TS.TypeOf(result.Value)
        //                .Ok(out var inferredType)
        //            ) result.Type = inferredType;
        //    });

        //    return Result.Ok(result);
        //}
    }
}
