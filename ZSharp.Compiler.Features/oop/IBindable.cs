namespace ZSharp.Compiler.Features.OOP
{
    public interface IBindable
    {
        public IResult Bind(Compiler compiler, CompilerObject @object);
    }
}
