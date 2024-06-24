using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal sealed class CTProcessor(ZSCompiler compiler) : IProcessor<CTType>
    {
        public ZSCompiler Compiler { get; } = compiler;

        public ICTBinding Assign(ICTBinding target, ICTBinding value)
        {
            return new ZSObjectBinding(target.Write(Compiler, value));
        }

        public ICTBinding Call(ICTBinding callee, Argument<CTType>[] arguments)
        {
            var type = callee.Type;

            if (type is VM.Types.FunctionType fType)
                type = new RTFunctionType(fType.Signature);
            if (type is not ICTCallable callable)
                throw new NotImplementedException();

            return new ZSObjectBinding(callable.Call(Compiler, callee.Read(Compiler), arguments));
        }

        public ICTBinding Cast(ICTBinding instance, ICTBinding targetType)
        {
            throw new NotImplementedException();
        }

        public ICTBinding Literal(object value, RLiteralType literalType, ICTBinding? unitType)
        {
            return new ZSObjectBinding(literalType switch
            {
                RLiteralType.String => new VM.ZSString((string)value),
                _ => throw new NotImplementedException(),
            });
        }
    }
}
