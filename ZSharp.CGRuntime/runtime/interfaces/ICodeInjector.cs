namespace ZSharp.CGRuntime
{
    public interface ICodeInjector
    {
        public CGObject CreateInjector(HLVM.Injector injector);
    }
}
