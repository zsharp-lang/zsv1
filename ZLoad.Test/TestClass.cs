using ZSharp.Runtime.NET.IL2IR;

namespace ZLoad.Test
{
    public class TestClass
    {
        [Alias(Name = "id")]
        public T Id<T>(T v) => v;
    }
}
