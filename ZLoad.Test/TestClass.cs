using ZSharp.Runtime.NET.IL2IR;

namespace ZLoad.Test
{
    public class TestClass : BaseClass
    {
        [Alias(Name = "id")]
        public T Id<T>(T v) => v;

        public override void DoVirtual()
        {
            Console.WriteLine("TestClass::DoVirtual");
        }
    }
}
