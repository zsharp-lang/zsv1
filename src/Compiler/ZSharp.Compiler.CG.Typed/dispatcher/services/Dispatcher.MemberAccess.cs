namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public IResult Member(CompilerObject @object, MemberIndex index)
        {
            var result = @base.Member(@object, index);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd) && rtd.Is<IRTGetMember<MemberIndex>>(out var member))
                result = member.Member(@object, index);

            return result;
        }

        public IResult Member(CompilerObject @object, MemberIndex index, CompilerObject value)
        {
            var result = @base.Member(@object, index, value);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd) && rtd.Is<IRTSetMember<MemberIndex>>(out var member))
                result = member.Member(@object, index, value);

            return result;
        }

        public IResult Member(CompilerObject @object, MemberName name)
        {
            var result = @base.Member(@object, name);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd) && rtd.Is<IRTGetMember<MemberName>>(out var member))
                result = member.Member(@object, name);

            return result;
        }

        public IResult Member(CompilerObject @object, MemberName name, CompilerObject value)
        {
            var result = @base.Member(@object, name, value);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd) && rtd.Is<IRTSetMember<MemberName>>(out var member))
                result = member.Member(@object, name, value);

            return result;
        }
    }
}
