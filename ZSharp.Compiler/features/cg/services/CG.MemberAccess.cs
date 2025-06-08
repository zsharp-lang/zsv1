namespace ZSharp.Compiler
{
    public partial struct CG
    {
        public CompilerObjectResult Member<M>(CompilerObject @object, M member)
        {
            var result = CompilerObjectResult.Error($"Object does not support get member by {typeof(M).Name}");

            if (@object is ICTGetMember<M> ctGet)
                result = ctGet.Member(compiler, member);

            if (result.IsOk) return result;

            if (
                RuntimeDescriptor(@object, out var runtimeDescritpr)
                && runtimeDescritpr is IRTGetMember<M> rtGet
            )
                result = rtGet.Member(compiler, @object, member);

            if (result.IsOk) return result;

            return result;
        }

        public CompilerObjectResult Member<M>(CompilerObject @object, M member, CompilerObject value)
        {
            var result = CompilerObjectResult.Error($"Object does not support set member by {typeof(M).Name}");

            if (@object is ICTSetMember<M> ctGet)
                result = ctGet.Member(compiler, member, value);

            if (result.IsOk) return result;

            if (
                RuntimeDescriptor(@object, out var runtimeDescritpr)
                && runtimeDescritpr is IRTSetMember<M> rtGet
            )
                result = rtGet.Member(compiler, @object, member, value);

            if (result.IsOk) return result;

            return result;
        }

        /// <summary>
        /// The get member (.) operator.
        /// </summary>
        /// <param name="object"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public CompilerObjectResult Member(CompilerObject @object, MemberIndex index)
            => Member<MemberIndex>(@object, index);

        /// <summary>
        /// The set member (.=) operator.
        /// </summary>
        /// <param name="object"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CompilerObjectResult Member(CompilerObject @object, MemberIndex index, CompilerObject value)
            => Member<MemberIndex>(@object, index, value);

        /// <summary>
        /// The member (.) operator.
        /// </summary>
        /// <param name="object"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public CompilerObjectResult Member(CompilerObject @object, MemberName name)
            => Member<MemberName>(@object, name);

        /// <summary>
        /// The set member (.=) operator.
        /// </summary>
        /// <param name="object"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CompilerObjectResult Member(CompilerObject @object, MemberName name, CompilerObject value)
            => Member<MemberName>(@object, name, value);
    }
}
