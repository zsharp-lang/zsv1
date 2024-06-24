namespace ZSharp.CTRuntime
{
    public class Argument<T>(IBinding<T> target)
    {
        public string? Name { get; set; }

        public IBinding<T> Binding { get; set; } = target;

        public Argument(string? name, IBinding<T> target) : this(target)
        {
            Name = name;
        }
    }
}
