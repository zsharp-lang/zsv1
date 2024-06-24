//namespace ZSharp.IR
//{
//    public class ZSObjectBinding<T> : IBinding
//        where T : ZSObject
//    {
//        public T Object { get; }

//        public IType Type => Object?.ObjectType ?? TypeSystem.Void;

//        public ZSObjectBinding(T @object)
//        {
//            Object = @object;
//        }

//        public virtual CodeResult Read()
//        {
//            return CodeResult.FromObject(Object);
//        }

//        public virtual CodeResult Write(IBinding value)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class ZSObjectBinding : ZSObjectBinding<ZSObject>
//    {
//        public ZSObjectBinding(ZSObject @object)
//            : base(@object)
//        {

//        }
//    }
//}
