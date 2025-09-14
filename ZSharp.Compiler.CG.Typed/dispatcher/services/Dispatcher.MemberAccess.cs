namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public Result Member(CompilerObject @object, MemberIndex index)
        {
            var result = @base.GetMemberByIndex(@object, index);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTGetMember<MemberIndex>>(out var member))
                result = member.Member(compiler, @object, index);

            return result;
        }

        public Result Member(CompilerObject @object, MemberIndex index, CompilerObject value)
        {
            var result = @base.SetMemberByIndex(@object, index, value);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTSetMember<MemberIndex>>(out var member))
                result = member.Member(compiler, @object, index, value);

            return result;
        }

        public Result Member(CompilerObject @object, MemberName name)
        {
            var result = @base.GetMemberByName(@object, name);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTGetMember<MemberName>>(out var member))
                result = member.Member(compiler, @object, name);

            return result;
        }

        public Result Member(CompilerObject @object, MemberName name, CompilerObject value)
        {
            var result = @base.SetMemberByName(@object, name, value);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTSetMember<MemberName>>(out var member))
                result = member.Member(compiler, @object, name, value);

            return result;
        }
    }
}
