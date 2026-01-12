namespace ZSharp.ZSSourceCompiler
{
    public interface IMultipassCompiler
    {
        public void AddToNextPass(Action action);
    }
}
