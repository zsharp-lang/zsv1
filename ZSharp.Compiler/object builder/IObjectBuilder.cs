namespace ZSharp.Compiler
{
    internal interface IObjectBuilder<T>
    {
        public void Declare(T @object);

        public void Define(T @object);
    }
}
