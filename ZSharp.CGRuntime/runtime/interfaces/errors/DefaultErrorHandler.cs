namespace ZSharp.CGRuntime
{
    internal sealed class DefaultErrorHandler : IErrorHandler
    {
        public void CouldNotFindName(string name)
        {
            throw new NotImplementedException();
        }

        public void NameAlreadyExists(string name)
        {
            throw new NotImplementedException();
        }
    }
}
