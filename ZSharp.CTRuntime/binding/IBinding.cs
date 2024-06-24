namespace ZSharp.CTRuntime
{
    public interface IBinding<T>
    {
        public ZSType Type { get; }

        public T Cast(ZSCompiler compiler, ZSType type) => throw new NotImplementedException();

        public T Read(ZSCompiler compiler, ZSType? @as)
        {
            if (@as == null || @as == Type)
                return Read(compiler);
            return Cast(compiler, @as);
        }

        public T Read(ZSCompiler compiler);

        public T Write(ZSCompiler compiler, IBinding<T> value);
    }
}
