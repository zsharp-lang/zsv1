namespace ZSharp.Compiler.Dispatchers.Direct
{
    partial class Dispatcher
    {
        public Result Member(CompilerObject @object, MemberIndex index)
        {
            var result = @base.GetMemberByIndex(@object, index);

            if (result.IsError && @object.Is<ICTGetMember<MemberIndex>>(out var member))
                result = member.Member(compiler, index);

            return result;
        }

        public Result Member(CompilerObject @object, MemberIndex index, CompilerObject value)
        {
            var result = @base.SetMemberByIndex(@object, index, value);

            if (result.IsError && @object.Is<ICTSetMember<MemberIndex>>(out var member))
                result = member.Member(compiler, index, value);

            return result;
        }

        public Result Member(CompilerObject @object, MemberName name)
        {
            var result = @base.GetMemberByName(@object, name);

            if (result.IsError && @object.Is<ICTGetMember<MemberName>>(out var member))
                result = member.Member(compiler, name);

            return result;
        }

        public Result Member(CompilerObject @object, MemberName name, CompilerObject value)
        {
            var result = @base.SetMemberByName(@object, name, value);

            if (result.IsError && @object.Is<ICTSetMember<MemberName>>(out var member))
                result = member.Member(compiler, name, value);

            return result;
        }
    }
}
