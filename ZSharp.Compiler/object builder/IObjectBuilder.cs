namespace ZSharp.Compiler
{
    internal interface IObjectBuilder
    {
        public void Declare(CGObject @object);

        public void Define(CGObject @object);
    }
}
