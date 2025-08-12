using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Method 
        : IRDefinition
        , ICallable
        , IModuleMember
    {
        public Function UnderlyingFunction { get; set; }

        public string? Name {
            get => UnderlyingFunction.Name;
            set => UnderlyingFunction.Name = value;
        }

        public MethodAttributes Attributes { get; set; } = MethodAttributes.None;

        public Collection<GenericParameter> GenericParameters => UnderlyingFunction.GenericParameters;

        public bool HasGenericParameters => UnderlyingFunction.HasGenericParameters;

        public IType ReturnType
        {
            get => UnderlyingFunction.ReturnType;
            set => UnderlyingFunction.ReturnType = value;
        }

        public Signature Signature
        {
            get => UnderlyingFunction.Signature;
            set => UnderlyingFunction.Signature = value;
        }

        ICallableBody? ICallable.Body => UnderlyingFunction.Body;

        public VM.FunctionBody Body => UnderlyingFunction.Body;

        public bool HasBody => UnderlyingFunction.HasBody;

        public OOPType? Owner { get; set; }

        public Module? Module => Owner?.Module;

        public bool IsClass
        {
            get => (Attributes & MethodAttributes.ClassMethod) == MethodAttributes.ClassMethod;
            set => Attributes = value
                ? Attributes | MethodAttributes.ClassMethod
                : Attributes & ~MethodAttributes.ClassMethod;
        }

        public bool IsInstance
        {
            get => (Attributes & MethodAttributes.InstanceMethod) == MethodAttributes.InstanceMethod;
            set => Attributes = value
                ? Attributes | MethodAttributes.InstanceMethod
                : Attributes & ~MethodAttributes.InstanceMethod;
        }

        public bool IsStatic
        {
            get => (Attributes & MethodAttributes.StaticMethod) == MethodAttributes.StaticMethod;
            set => Attributes = value
                ? Attributes | MethodAttributes.StaticMethod
                : Attributes & ~MethodAttributes.StaticMethod;
        }

        public bool IsVirtual
        {
            get => (Attributes & MethodAttributes.VirtualMethod) == MethodAttributes.VirtualMethod;
            set => Attributes = value
                ? Attributes | MethodAttributes.VirtualMethod
                : Attributes & ~MethodAttributes.VirtualMethod;
        }

        public bool IsAbstract
        {
            get => (Attributes & MethodAttributes.Abstract) == MethodAttributes.Abstract;
            set
            {
                if (value) Attributes |= MethodAttributes.Abstract;
                else Attributes &= ~MethodAttributes.Abstract;
            }
        }

        public Method(IType returnType)
            : this(new Signature(returnType))
        {
            
        }

        public Method(Signature signature)
            : this(new Function(signature))
        {
            
        }

        public Method(Function function)
        {
            UnderlyingFunction = function;
        }
    }
}
