namespace CommonZ
{
    public interface IOwned<T> where T : class
    {
        public T? Owner { get; set; }
    }
}
