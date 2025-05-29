namespace ZSharp.Compiler
{
    public readonly struct CG(Compiler compiler)
    {
        public readonly Compiler compiler = compiler;

        public CompilerObjectResult Assign(CompilerObject target, CompilerObject value)
        {
            if (target is ICTAssignable ctAssignable)
                return CompilerObjectResult.Ok(
                    ctAssignable.Assign(compiler, value)
                );

            return CompilerObjectResult.Error(
                "Object does not support assignment"
            );
        }

        // TODO: WTH is this?
        public IRCode Assign(IRCode irCode, Assignment assignment)
            => throw new NotImplementedException();

        public CompilerObjectResult Call(CompilerObject target, Argument[] arguments)
        {
            CompilerObject? result = null;

            if (target is ICTCallable ctCallable)
                result = ctCallable.Call(compiler, arguments);

            else if (target is ICTReadable readable)
                if (readable.Type is IRTCallable rtCallable)
                    result = rtCallable.Call(compiler, target, arguments);

            // implements typeclass Callable?

            // overloads call operator?

            if (result is not null)
                return CompilerObjectResult.Ok(result);
            return CompilerObjectResult.Error(
                "Object is not callable"
            );
        }

        public Result<TypeCast, Error> Cast(CompilerObject value, IType type)
        {
            var result = Result<TypeCast, Error>.Error(
                "Object cannot be cast"
            );

            if (
                compiler.TypeSystem.IsTyped(value, out var valueType)
                && valueType is IRTCastTo rtCastTo
            )
                result = rtCastTo.Cast(compiler, value, type);

            if (result.IsOk) return result;

            if (type is IRTCastFrom rtCastFrom)
                result = rtCastFrom.Cast(compiler, value);

            if (result.IsOk) return result;

            if (valueType is not null && compiler.TypeSystem.AreEqual(type, valueType))
                result = Result<TypeCast, Error>.Ok(new()
                {
                    Cast = value,
                });

            return result;
        }

        /// <summary>
        /// The get index ([]) operator.
        /// </summary>
        /// <param name="instanceTarget"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public CompilerObject? Index(CompilerObject instanceTarget, Argument[] index)
        {
            if (instanceTarget is ICTGetIndex ctGetIndex)
                return ctGetIndex.Index(compiler, index);

            return null;
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

        public CompilerObject Map(CompilerObject @object, Func<CompilerObject, CompilerObject> func)
        {
            if (@object is IMappable mappable)
                return mappable.Map(func);

            return func(@object);
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
                return ctGetMember.Member(compiler, member);

            if (instance is ICTReadable readable && readable.Type is IRTGetMember<MemberName> rtGetMember)
                return rtGetMember.Member(compiler, instance, member);

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

        public Result<TypeMatch, Error> TypeMatch(CompilerObject @object, IType type)
        {
            var result = Result<TypeMatch, Error>.Error(
                "Object cannot be type matched"
            );

            if (@object is ICTTypeMatch ctTypeMatch)
                result = ctTypeMatch.Match(compiler, type);

            if (result.IsOk) return result;

            if (
                compiler.TypeSystem.IsTyped(@object, out var objectType) 
                && objectType is IRTTypeMatch rtTypeMatch
            )
                result = rtTypeMatch.Match(compiler, @object, type);

            if (result.IsOk) return result;

            return result;
        }
    }
}
