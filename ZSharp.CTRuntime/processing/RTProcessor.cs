using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal class RTProcessor(ZSCompiler compiler) : IProcessor<RTType>
    {
        public ZSCompiler Compiler { get; } = compiler;

        public IRTBinding Assign(IRTBinding target, IRTBinding value)
        {
            return new CodeBinding(target.Write(Compiler, value));
        }

        public IRTBinding Call(IRTBinding callee, Argument<RTType>[] arguments)
        {
            var type = callee.Type;

            if (type is VM.Types.FunctionType fType)
                type = new RTFunctionType(fType.Signature);

            if (type is not IRTCallable callable)
                throw new NotImplementedException();

            return new CodeBinding(callable.Call(Compiler, callee.Read(Compiler), arguments));
        }

        public IRTBinding Cast(IRTBinding instance, IRTBinding targetType)
        {
            throw new NotImplementedException();
        }

        public IRTBinding Literal(object value, RLiteralType literalType, IRTBinding? unitType)
        {
            (IR.VM.Put instruction, VM.ZSObject type) = literalType switch
            {
                RLiteralType.String => (new IR.VM.PutString((string)value!), VM.TypeSystem.String),
                RLiteralType.Integer => throw new NotImplementedException(),
                RLiteralType.Real => throw new NotImplementedException(),
                RLiteralType.Boolean => throw new NotImplementedException(),
                RLiteralType.Null => throw new NotImplementedException(),
                RLiteralType.Unit => throw new NotImplementedException(),
                RLiteralType.I8 => throw new NotImplementedException(),
                RLiteralType.I16 => throw new NotImplementedException(),
                RLiteralType.I32 => throw new NotImplementedException(),
                RLiteralType.I64 => throw new NotImplementedException(),
                RLiteralType.U8 => throw new NotImplementedException(),
                RLiteralType.U16 => throw new NotImplementedException(),
                RLiteralType.U32 => throw new NotImplementedException(),
                RLiteralType.U64 => throw new NotImplementedException(),
                RLiteralType.F32 => throw new NotImplementedException(),
                RLiteralType.F64 => throw new NotImplementedException(),
                RLiteralType.I => throw new NotImplementedException(),
                RLiteralType.U => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
            IRTBinding result = new CodeBinding(new(1, [instruction], type));

            if (unitType is not null)
                result = Call(unitType, [ new(result) ]);

            return result;
        }
    }
}
