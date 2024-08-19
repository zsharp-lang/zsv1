using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed class CodeGenerator(ZSCompiler compiler)
        : ICodeGenerator
    {
        private readonly ZSCompiler compiler = compiler;

        /// <summary>
        /// The assigment (=) operator.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CTObject Assign(CTObject target, CTObject value)
        {
            if (target is ICTReadable readable)
                if (readable.Type is IRTAssignable rtWritable)
                    return rtWritable.Assign(compiler, Read(readable), value);

            if (target is ICTAssignable ctWritable)
                return ctWritable.Assign(compiler, value);

            // call the assign operator with (target.Read, value)

            throw new("Assignment operator is not implemented for this type.");
        }

        /// <summary>
        /// The call (()) operator.
        /// </summary>
        /// <param name="callee"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public CTObject Call(CTObject callee, Argument[] arguments)
        {
            if (callee is ICTReadable readable)
                if (readable.Type is IRTCallable rtCallable)
                    return rtCallable.Call(compiler, Read(readable), arguments);

            if (callee is ICTCallable ctCallable)
                return ctCallable.Call(compiler, arguments);

            // implements typeclass Callable?

            // overloads call operator?

            throw new("Object of this type is not callable.");
        }

        /// <summary>
        /// The cast (as/()) operator.
        /// </summary>
        /// <param name="instanceTarget"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public CTObject Cast(CTObject instanceTarget, CTObject targetType)
        {
            if (instanceTarget is ICTReadable readable)
                if (readable.Type is IRTTypeCast rtTypeCast)
                    return rtTypeCast.Cast(compiler, Read(readable), targetType);
                else
                    return readable.Read(compiler, targetType);

            if (instanceTarget is ICTTypeCast ctTypeCast)
                return ctTypeCast.Cast(compiler, targetType);

            // implements typeclass TypeCast?

            // overloads cast operator?

            throw new("Object of this type is not castable.");
        }

        /// <summary>
        /// The get index ([]) operator.
        /// </summary>
        /// <param name="instanceTarget"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public CTObject Index(CTObject instanceTarget, Argument[] index)
        {
            throw new NotImplementedException();

            //IGetIndex<T>? indexed = instanceTarget as IGetIndex<T> ?? instanceTarget.Type as IGetIndex<T>;

            //if (indexed is not null)
            //    return indexed.Index(Read(instanceTarget), index);

            // overloads the index operator?

            throw new("Object of this type does not support the index operator.");
        }

        /// <summary>
        /// The set index ([]=) operator.
        /// </summary>
        /// <param name="instanceTarget"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CTObject Index(CTObject instanceTarget, Argument[] index, CTObject value)
        {
            throw new NotImplementedException();

            //ISetIndex<T>? indexed = instanceTarget as ISetIndex<T> ?? instanceTarget.Type as ISetIndex<T>;

            //if (indexed is not null)
            //    return indexed.Index(Read(instanceTarget), index, value);

            // overloads the index operator?

            throw new("Object of this type does not support the index operator.");
        }

        /// <summary>
        /// The member (.) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public CTObject Member(CTObject instance, MemberIndex index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set member (.=) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CTObject Member(CTObject instance, MemberIndex index, CTObject value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The member (.) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public CTObject Member(CTObject instance, MemberName member)
        {
            if (instance is ICTReadable readable)
                if (readable.Type is IRTGetMember<MemberName> rtGetMember)
                    return rtGetMember.Member(compiler, Read(readable), member);

            if (instance is ICTGetMember<MemberName> ctGetMember)
                return ctGetMember.Member(compiler, member);

            // overloads the member(this, string) operator?

            throw new("Object of this type does not support the member operator.");
        }

        /// <summary>
        /// The set member (.=) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CTObject Member(CTObject instance, MemberName member, CTObject value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The value of the object.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        //Code Value(IBinding<T> target);

        public Code Read(CTObject @object) => 
            @object is Code code ? code :
            @object is ICTReadable readable ? Read(readable) : 
            throw new();

        public Code Read(ICTReadable readable) => readable.Read(compiler);

        //private static U? Protocol<U>(IBinding<T> binding)
        //    where U : class
        //    => binding.Type as U ?? binding as U;

        //Code Literal(object value, RLiteralType literalType, IBinding<T>? unitType);

        public CTObject Literal(object value, RLiteralType literalType, CTObject? unitType)
        {
            (IR.VM.Put instruction, ZSType type) = literalType switch
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
                RLiteralType.Imaginary => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
            CTObject result = new Code(1, [instruction], type);

            if (unitType is not null)
                result = Call(unitType, [new(result)]);

            return result;
        }
    }
}
