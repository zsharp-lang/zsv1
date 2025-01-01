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

        public Class? Base { get; set; }

        //public SimpleFunctionOverloadGroup? Constructor { get; set; }

        public Constructor? Constructor { get; set; }

        public Constructor? EmptyConstructor { get; set; }

        public Method? ClassConstructor { get; set; }

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (arguments.Length == 0)
                if (EmptyConstructor is null) throw new InvalidOperationException();
                else return compiler.Call(EmptyConstructor, arguments);

            return new RawCode(
                new([
                    new IR.VM.CreateInstance(Constructor!.IR!)
                ])
                {
                    MaxStackSize = 1,
                    Types = [this]
                });
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
