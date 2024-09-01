namespace ZSharp.CGRuntime
{
    public interface IErrorHandler
    {
        public void CouldNotFindName(string name);

        public void NameAlreadyExists(string name);
    }
}
