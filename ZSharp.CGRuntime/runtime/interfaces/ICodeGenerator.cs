namespace ZSharp.CGRuntime
{
    public interface ICodeGenerator
    {
        /// <summary>
        /// The assigment (=) operator.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CGObject Assign(CGObject target, CGObject value);

        /// <summary>
        /// The call (()) operator.
        /// </summary>
        /// <param name="callee"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public CGObject Call(CGObject callee, Argument[] arguments);

        /// <summary>
        /// The cast (as/()) operator.
        /// </summary>
        /// <param name="instanceTarget"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public CGObject Cast(CGObject instanceTarget, CGObject targetType);

        /// <summary>
        /// The get index ([]) operator.
        /// </summary>
        /// <param name="instanceTarget"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public CGObject Index(CGObject instanceTarget, Argument[] index);

        /// <summary>
        /// The set index ([]=) operator.
        /// </summary>
        /// <param name="instanceTarget"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CGObject Index(CGObject instanceTarget, Argument[] index, CGObject value);

        /// <summary>
        /// The member (.) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public CGObject Member(CGObject instance, MemberIndex index);

        /// <summary>
        /// The set member (.=) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CGObject Member(CGObject instance, MemberIndex index, CGObject value);

        /// <summary>
        /// The member (.) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public CGObject Member(CGObject instance, MemberName member);

        /// <summary>
        /// The set member (.=) operator.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CGObject Member(CGObject instance, MemberName member, CGObject value);

        public CGObject Literal(object value, RAST.RLiteralType literalType, CGObject? unitType);
    }
}
