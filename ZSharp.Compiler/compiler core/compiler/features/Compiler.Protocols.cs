namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public CompilerObject Assign(CompilerObject target, CompilerObject value)
        {
            if (target is ICTAssignable ctAssignable)
                return ctAssignable.Assign(this, value);

            throw new NotImplementedException();
        }

        // TODO: WTH is this?
        public IRCode Assign(IRCode irCode, Assignment assignment)
            => throw new NotImplementedException();

        public CompilerObject Call(CompilerObject target, Argument[] arguments)
        {
            if (target is ICTCallable ctCallable)
                return ctCallable.Call(this, arguments);

            if (target is ICTReadable readable)
                if (readable.Type is IRTCallable rtCallable)
                    return rtCallable.Call(this, target, arguments);

            // implements typeclass Callable?

            // overloads call operator?

            throw new("Object of this type is not callable.");
        }

        public CompilerObject Cast(CompilerObject target, CompilerObject type)
        {
            if (TypeSystem.IsTyped(target, out var targetType) && targetType == type)
                return target;

            if (target is ICTTypeCast typeCast)
                return typeCast.Cast(this, type);

            throw new NotImplementedException();
        }

        /// <summary>
        /// The get index ([]) operator.
        /// </summary>
        /// <param name="instanceTarget"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public CompilerObject Index(CompilerObject instanceTarget, Argument[] index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set index ([]=) operator.
        /// </summary>
        /// <param name="instanceTarget"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CompilerObject Index(CompilerObject instanceTarget, Argument[] index, CompilerObject value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The member (.) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public CompilerObject Member(CompilerObject instance, MemberIndex index)
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
        public CompilerObject Member(CompilerObject instance, MemberIndex index, CompilerObject value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The member (.) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public CompilerObject Member(CompilerObject instance, MemberName member)
        {
            if (instance is ICTGetMember<MemberName> ctGetMember)
                return ctGetMember.Member(this, member);

            if (instance is ICTReadable readable && readable.Type is IRTGetMember<MemberName> rtGetMember)
                return rtGetMember.Member(this, instance, member);

            throw new NotImplementedException();
        }

        /// <summary>
        /// The set member (.=) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CompilerObject Member(CompilerObject instance, MemberName member, CompilerObject value)
        {
            throw new NotImplementedException();
        }
    }
}
