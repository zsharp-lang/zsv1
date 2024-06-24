//namespace ZSharp.CTRuntime
//{
//    internal sealed class TypeBinding(VM.IType type) : IBinding, VM.IType
//    {
//        public VM.IType TypeObject => type;

//        public VM.IType Type { get; } = TypeSystem.Type;

//        public Code Read()
//        {
//            return new(1, [new IR.VM.GetObject(TypeObject)], Type);
//        }

//        public Code Write(IBinding value)
//        {
//            throw new NotImplementedException();
//        }

//        public bool? IsAssignableTo(VM.IType target)
//        {
//            return TypeObject.IsAssignableTo(target);
//        }

//        public bool? IsAssignableFrom(VM.IType source)
//        {
//            return TypeObject.IsAssignableFrom(source);
//        }
//    }
//}
