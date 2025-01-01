using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public interface IMetaClass : ICTCallable
    {
        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (arguments.Length != 3)
                throw new($"Metaclass may only be called with exactly 3 arguments");

            foreach (var argument in arguments)
                if (argument.Name is not null)
                    throw new($"Metaclass's arguments must not have names");

            if (!compiler.IsCTValue<string>(arguments[0].Object, out var name))
                throw new($"Metaclass's first argument must be a string literal");

            if (!compiler.IsCTValue<CompilerObject[]>(arguments[1].Object, out var bases))
                throw new($"Metaclass's second argument must be an array of bases");

            if (!compiler.IsCTValue<CompilerObject[]>(arguments[2].Object, out var content))
                throw new($"Metaclass's third argument must be an array of content");

            return Construct(compiler, name, bases, content);
        }

        public CompilerObject Construct(Compiler.Compiler compiler, string name, CompilerObject[] bases, CompilerObject[]? content);
    }

    public interface IMetaClass<T> : IMetaClass
        where T : CompilerObject
    {
        CompilerObject IMetaClass.Construct(Compiler.Compiler compiler, string name, CompilerObject[] bases, CompilerObject[]? content)
            => Construct(compiler, name, bases, content);

        public new T Construct(Compiler.Compiler compiler, string name, CompilerObject[] bases, CompilerObject[]? content);
    }
}
