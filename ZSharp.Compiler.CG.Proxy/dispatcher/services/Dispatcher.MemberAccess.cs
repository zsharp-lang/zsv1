namespace ZSharp.Compiler.CGDispatchers.Proxy
{
    partial class Dispatcher
    {
        public Result Member(CompilerObject @object, MemberIndex index)
        {
            var result = @base.GetMemberByIndex(@object, index);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Member(proxied, index));

            return result;
        }

        public Result Member(CompilerObject @object, MemberIndex index, CompilerObject value)
        {
            var result = @base.SetMemberByIndex(@object, index, value);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Member(proxied, index, value));

            return result;
        }

        public Result Member(CompilerObject @object, MemberName name)
        {
            var result = @base.GetMemberByName(@object, name);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Member(proxied, name));

            return result;
        }

        public Result Member(CompilerObject @object, MemberName name, CompilerObject value)
        {
            var result = @base.SetMemberByName(@object, name, value);

            if (result.IsError && @object.Is<IProxy>(out var proxy))
                result = proxy.Apply(proxied => CG.Member(proxied, name, value));

            return result;
        }
    }
}
