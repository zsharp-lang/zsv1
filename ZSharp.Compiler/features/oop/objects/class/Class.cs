using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Class
        : CompilerObject
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , ICTCallable
    {
        public Mapping<string, CompilerObject> Members { get; } = [];

        public IR.Class? IR { get; set; }

        public string? Name { get; set; }

        public CompilerObject? Base { get; set; }

        //public SimpleFunctionOverloadGroup? Constructor { get; set; }

        public CompilerObject? Constructor { get; set; }

        public Constructor? EmptyConstructor { get; set; }

        public Method? ClassConstructor { get; set; }

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (arguments.Length == 0)
                if (EmptyConstructor is null) throw new InvalidOperationException();
                else return compiler.Call(EmptyConstructor, arguments);

            if (Constructor is null) throw new InvalidOperationException();
            return compiler.Call(Constructor, arguments);
        }

        //public List<Interface>? Interfaces { get; set; }

        public CompilerObject Member(Compiler.Compiler compiler, MemberName member)
            => Members[member];

        public CompilerObject Member(Compiler.Compiler compiler, CompilerObject value, MemberName member)
            => Member(compiler, member) is IRTBoundMember rtBoundMember
                ? rtBoundMember.Bind(compiler, value)
                : throw new NotImplementedException();
    }
}
