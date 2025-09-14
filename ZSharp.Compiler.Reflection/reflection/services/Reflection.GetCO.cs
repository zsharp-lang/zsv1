namespace ZSharp.Compiler
{
    public delegate Result GetCO(object @object);

    partial struct Reflection
    {
        public GetCO GetCO { get; set; } = Dispatcher.GetCO;
    }
}
