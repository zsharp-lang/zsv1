using CommonZ.Utils;
using ZSharp.CGObjects;
using ZSharp.VM;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private readonly Mapping<IRType, NullLiteral> nullLiterals = [];

        public CGObject CreateFloat32(float value)
            => new Float32Literal(value, RuntimeModule.TypeSystem.Float32);

        public CGObject CreateInteger(DefaultIntegerType value)
            => new IntegerLiteral(value, RuntimeModule.TypeSystem.Int32); // TODO: fix type here

        public CGObject CreateRuntimeObject(ZSObject @object)
            => new RuntimeObject(@object);

        public CGObject CreateString(string value)
            => new StringLiteral(value, RuntimeModule.TypeSystem.String);

        public CGObject CreateNull(IRType type)
        {
            if (!nullLiterals.TryGetValue(type, out var nullLiteral))
                nullLiterals[type] = nullLiteral = new(type);

            return nullLiteral;
        }

        public CGObject CreateLiteral()
        {
            throw new NotImplementedException();
            //(IR.VM.Put instruction, IRType type) = literalType switch
            //{
            //    RLiteralType.String => (new IR.VM.PutString((string)value!), compiler.RuntimeModule.TypeSystem.String),
            //    RLiteralType.Integer => throw new NotImplementedException(),
            //    RLiteralType.Real => throw new NotImplementedException(),
            //    RLiteralType.Boolean => throw new NotImplementedException(),
            //    RLiteralType.Null => throw new NotImplementedException(),
            //    RLiteralType.Unit => throw new NotImplementedException(),
            //    RLiteralType.I8 => throw new NotImplementedException(),
            //    RLiteralType.I16 => throw new NotImplementedException(),
            //    RLiteralType.I32 => throw new NotImplementedException(),
            //    RLiteralType.I64 => throw new NotImplementedException(),
            //    RLiteralType.U8 => throw new NotImplementedException(),
            //    RLiteralType.U16 => throw new NotImplementedException(),
            //    RLiteralType.U32 => throw new NotImplementedException(),
            //    RLiteralType.U64 => throw new NotImplementedException(),
            //    RLiteralType.F32 => throw new NotImplementedException(),
            //    RLiteralType.F64 => throw new NotImplementedException(),
            //    RLiteralType.I => throw new NotImplementedException(),
            //    RLiteralType.U => throw new NotImplementedException(),
            //    RLiteralType.Imaginary => throw new NotImplementedException(),
            //    _ => throw new NotImplementedException(),
            //};
            //CGObject result = new RawCode(new()
            //{
            //    Instructions = [instruction],
            //    Types = [type],
            //    MaxStackSize = 1
            //});

            //if (unitType is not null)
            //    result = Call(unitType, [new(result)]);

            //return result;
        }
    }
}
