namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public CGObject Assign(CGObject target, CGObject value)
        {
            throw new NotImplementedException();
        }

        public CGObject Call(CGObject target, CGRuntime.Argument[] arguments)
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
        public CGObject Index(CGObject instanceTarget, CGRuntime.Argument[] index)
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
        public CGObject Index(CGObject instanceTarget, CGRuntime.Argument[] index, CGObject value)
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
            if (instance is ICTGetMember<MemberName> getMember)
                return getMember.Member(this, member);

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
