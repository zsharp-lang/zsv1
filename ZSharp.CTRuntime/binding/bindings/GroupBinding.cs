//using CommonZ.Utils;

//namespace ZSharp.IR
//{
//    public class GroupBinding : ZSObjectBinding<FunctionGroup>, ICallable
//    {
//        public Collection<FunctionBinding> Overloads { get; }

//        public GroupBinding(FunctionGroup group)
//            : base(group)
//        {
//            Overloads = new Collection<FunctionBinding>(group.Overloads.Select(overload => new FunctionBinding(overload)));
//        }

//        public CodeResult Call(IBinding callable, Argument[] arguments)
//        {
//            foreach (var overload in Overloads)
//            {
//                try
//                {
//                    return overload.Type.Call(overload, arguments);
//                }
//                catch (Exception e)
//                    when (e.Message == "NoOverload")
//                {
//                    continue;
//                }
//            }

//            throw new Exception();
//        }
//    }
//}
