namespace ZSharp.VM.Types
{
    internal sealed class Type : PrimitiveType // todo: inherit ZSClass
    {
        public class TypeMethods
        {
            public IR.Method IsInstance { get; }

            public IR.Method IsAssignableTo { get; }

            public IR.Method IsAssignableFrom { get; }
        }

        public TypeMethods Methods { get; }

        public Type()
            : base("Type")
        {
            Methods = new()
            {
                
            };
        }

        public override bool? IsAssignableFrom(Interpreter interpreter, ZSObject source)
        {
            if (interpreter.IsInstance(source, this as IType)) return true;
            return null;
        }

        public override bool? IsAssignableTo(Interpreter interpreter, ZSObject target)
        {
            if (target == this) return true;
            return null;
        }
    }
}
