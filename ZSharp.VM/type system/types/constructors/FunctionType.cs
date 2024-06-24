namespace ZSharp.VM.Types
{
    public sealed class FunctionType(ZSSignature signature) 
        : PrimitiveType("Function")
    {
        public ZSSignature Signature { get; } = signature;

        public override bool? IsAssignableFrom(Interpreter interpreter, ZSObject source)
        {
            if (source is not FunctionType other)
                return false;

            // TODO: Compare signatures

            // TODO: Compare return types

            return true;
        }

        public override bool? IsAssignableTo(Interpreter interpreter, ZSObject target)
        {
            if (target is not FunctionType other)
                return false; // TODO: function is assignable to callables

            return true;
        }
    }
}
