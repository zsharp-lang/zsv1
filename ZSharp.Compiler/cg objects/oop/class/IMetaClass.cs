using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public interface ICTMetaClass : ICTCallable
    {
        CGObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (arguments.Length != 1)
                throw new($"Metaclass may only be called with a single argument");

            if (arguments[0].Name is not null)
                throw new($"Argument to metaclass must not be a keyword argument");

            if (arguments[0].Object is not ClassSpec spec)
                throw new($"Argument to metaclass must be a {nameof(ClassSpec)} instance.");

            return Construct(compiler, spec);
        }

        public CGObject Construct(Compiler.Compiler compiler, ClassSpec spec);

        public void Compile(Compiler.Compiler compiler, CGObject result, ClassSpec spec);
    }

    public interface ICTMetaClass<T> : ICTMetaClass
        where T : CGObject
    {
        CGObject ICTMetaClass.Construct(Compiler.Compiler compiler, ClassSpec spec)
            => Construct(compiler, spec);

        public new T Construct(Compiler.Compiler compiler, ClassSpec spec);

        public void Compile(Compiler.Compiler compiler, T result, ClassSpec spec);

        void ICTMetaClass.Compile(Compiler.Compiler compiler, CGObject result, ClassSpec spec)
        {
            if (result is not T value)
                throw new ArgumentException($"Argument {nameof(result)} must be an instance of {typeof(T).Name}", nameof(result));

            Compile(compiler, value, spec);
        }
    }
}
