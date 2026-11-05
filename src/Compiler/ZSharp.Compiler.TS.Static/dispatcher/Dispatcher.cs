namespace ZSharp.Compiler.TSDispatchers.Static
{
    public sealed class Dispatcher(TS ts)
    {
        private readonly TS ts = ts;
        private TS @base = null!;

        public void Apply()
        {
            @base = ts.Clone();

            ts.TypeOf = TypeOf;
        }

        public IResult TypeOf(CompilerObject @object)
        {
            var result = @base.TypeOf(@object);

            if (result.IsUnsupported() && @object.Is<ITyped>(out var typed))
                result = Result.Ok(typed.Type);

            return result;
        }
    }
}
