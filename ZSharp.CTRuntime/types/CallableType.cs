//namespace ZSharp.IR
//{
//    public abstract class CallableType<T> : ICallable
//        where T : class
//    {
//        public CodeResult Call(IBinding callable, Argument[] arguments)
//        {
//            if (callable is not T @object)
//                throw new Exception();
//            return Call(@object, arguments);
//        }

//        public abstract CodeResult Call(T callable, Argument[] arguments);
//    }
//}
