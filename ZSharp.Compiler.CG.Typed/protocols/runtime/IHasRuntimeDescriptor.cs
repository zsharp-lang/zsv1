namespace ZSharp.Compiler
{
    public interface IHasRuntimeDescriptor
    {
        public IResult GetRuntimeDescriptor(Compiler compiler);
    }
}
