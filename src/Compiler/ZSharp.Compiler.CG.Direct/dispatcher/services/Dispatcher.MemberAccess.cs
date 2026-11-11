namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult Member(CompilerObject @object, MemberIndex index)
        {
            var result = @base.Member(@object, index);

            if (result.IsUnsupported() && @object.Is<ICTGetMember<MemberIndex>>(out var member))
                result = member.Member(index);

            return result;
        }

        public IResult Member(CompilerObject @object, MemberIndex index, CompilerObject value)
        {
            var result = @base.Member(@object, index, value);

            if (result.IsUnsupported() && @object.Is<ICTSetMember<MemberIndex>>(out var member))
                result = member.Member(index, value);

            return result;
        }

        public IResult Member(CompilerObject @object, MemberName name)
        {
            var result = @base.Member(@object, name);

            if (result.IsUnsupported() && @object.Is<ICTGetMember<MemberName>>(out var member))
                result = member.Member(name);

            return result;
        }

        public IResult Member(CompilerObject @object, MemberName name, CompilerObject value)
        {
            var result = @base.Member(@object, name, value);

            if (result.IsUnsupported() && @object.Is<ICTSetMember<MemberName>>(out var member))
                result = member.Member(name, value);

            return result;
        }
    }
}
