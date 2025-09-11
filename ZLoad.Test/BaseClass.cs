using ZSharp.Runtime.NET.IL2IR;

namespace ZLoad.Test
{
    public class BaseClass
    {
        [Alias(Name = "doVirtual")]
        public virtual void DoVirtual()
        {
            Console.WriteLine("BaseClass::DoVirtual()");
        }
    }
}
