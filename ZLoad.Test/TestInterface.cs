using ZSharp.Runtime.NET.IL2IR;

namespace ZLoad.Test
{
    public interface TestInterface
    {
        [Alias(Name = "do")]
        public void Do();

        [Alias(Name = "do")]
        public int Calc(int x);
    }
}
