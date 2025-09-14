namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result GetCO(object @object)
        {
            if (@object is CompilerObject co)
                return Result.Ok(co);

            return Result.Error(
                $"Object [{@object}] does not support static reflection."
            );
        }
    }
}
