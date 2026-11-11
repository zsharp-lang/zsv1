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
            ts.IsAssignableTo = IsAssignableTo;
        }

        /// <summary>
        /// Assignability goes both ways: the source can say where it goes, and the
        /// target can say what it takes.
        /// </summary>
        public bool IsAssignableTo(CompilerObject target, CompilerObject source)
            => @base.IsAssignableTo(target, source)
            || source.Is<IAssignableTo>(out var to) && to.IsAssignableTo(target)
            || target.Is<IAssignableFrom>(out var from) && from.IsAssignableFrom(source);

        public IResult TypeOf(CompilerObject @object)
        {
            var result = @base.TypeOf(@object);

            if (result.IsUnsupported() && @object.Is<ITyped>(out var typed))
                result = Result.Ok(typed.Type);

            return result;
        }
    }
}
