namespace ZSharp.Compiler
{
    internal static class Dispatcher
    {
        public static IResult Call(CompilerObject callee, Argument[] arguments)
            => Unsupported($"Object [{callee}] does not support call operation.");

        public static IResult MatchParameter(CompilerObject parameter, ArgumentStream arguments)
            => Unsupported($"Object [{parameter}] does not support matching arguments.");

        public static IResult Declare(CompilerObject definition)
            => Unsupported($"Object [{definition}] does not support declaration.");

        public static IResult Cast(CompilerObject @object, CompilerObject type)
            => Unsupported($"Object [{@object}] does not support casting to [{type}].");

        public static IResult ImplicitCast(CompilerObject @object, CompilerObject type)
            => Unsupported($"Object [{@object}] does not support implicit casting to [{type}].");

        public static IResult Index(CompilerObject @object, Argument[] arguments)
            => Unsupported($"Object [{@object}] does not support index operation.");

        public static IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
            => Unsupported($"Object [{@object}] does not support index assignment operation.");

        public static IResult<MatchResult, Error> Match(CompilerObject @object, CompilerObject pattern)
            => Result<MatchResult>.Error(
                new UnsupportedError($"Object [{@object}] does not support pattern matching.")
            );

        public static IResult Member(CompilerObject @object, MemberIndex index)
            => Unsupported($"Object [{@object}] does not support member access by index.");

        public static IResult Member(CompilerObject @object, MemberIndex index, CompilerObject value)
            => Unsupported($"Object [{@object}] does not support member assignment by index.");

        public static IResult Member(CompilerObject @object, MemberName name)
            => Unsupported($"Object [{@object}] does not support member access by name.");

        public static IResult Member(CompilerObject @object, MemberName name, CompilerObject value)
            => Unsupported($"Object [{@object}] does not support member assignment by name.");

        public static IResult Get(CompilerObject @object)
            => Unsupported($"Object [{@object}] does not support get.");

        public static IResult Set(CompilerObject @object, CompilerObject value)
            => Unsupported($"Object [{@object}] does not support set.");

        private static IResult Unsupported(string message)
            => Result.Error(new UnsupportedError(message));
    }
}
