namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public CGObject Assign(CGObject target, CGObject value)
        {
            if (target is ICTAssignable ctAssignable)
                return ctAssignable.Assign(this, value);

            throw new NotImplementedException();
        }

        // TODO: WTH is this?
        public Code Assign(Code irCode, Assignment assignment)
            => throw new NotImplementedException();

        public CGObject Call(CGObject target, Argument[] arguments)
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

        public CGObject Cast(CGObject target, CGObject type)
        {
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
        public CGObject Index(CGObject instanceTarget, Argument[] index)
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
        public CGObject Index(CGObject instanceTarget, Argument[] index, CGObject value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The member (.) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public CGObject Member(CGObject instance, MemberIndex index)
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
        public CGObject Member(CGObject instance, MemberIndex index, CGObject value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The member (.) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public CGObject Member(CGObject instance, MemberName member)
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
        public CGObject Member(CGObject instance, MemberName member, CGObject value)
        {
            throw new NotImplementedException();
        }
    }
}
