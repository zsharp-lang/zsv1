namespace ZSharp.Compiler.Dispatchers.Direct
{
    partial class Dispatcher
    {
        public Result TypeOf(CompilerObject @object)
        {
            var result = @base.TypeOf(@object);

            if (result.IsError && @object.Is<ITyped>(out var typed))
                result = Result.Ok(typed.Type);

            return result;
        }
    }
}
