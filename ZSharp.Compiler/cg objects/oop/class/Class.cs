using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Class
        : CGObject
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , ICTCallable
    {
        public Mapping<string, CGObject> Members { get; } = [];

        public IR.Class? IR { get; set; }

        public string? Name { get; set; }

        public Class? Base { get; set; }

        public Method? Constructor { get; set; }

        public CGObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (Constructor is null)
            {
                IR!.Methods.Add((Constructor = new(null)
                {
                    IR = new(compiler.RuntimeModule.TypeSystem.Void)
                    {
                        Owner = IR
                    },
                    ReturnType = compiler.TypeSystem.Void,
                    Signature = new()
                    {
                        Args = [
                                new("this") {
                                IR = new("this", IR),
                                Type = this
                            }
                            ]
                    }
                }).IR);

                Constructor.IR.Body.Instructions.Add(new IR.VM.Return());
            }

            return new RawCode(
                new([
                    new IR.VM.CreateInstance(new() {
                        Method = Constructor!.IR!,
                    })
                ])
                {
                    MaxStackSize = 1,
                    Types = [this]
                });
        }

        //public List<Interface>? Interfaces { get; set; }

        public CGObject Member(Compiler.Compiler compiler, MemberName member)
            => Members[member];

        public CGObject Member(Compiler.Compiler compiler, CGObject value, MemberName member)
            => Member(compiler, member) is IRTBoundMember rtBoundMember
                ? rtBoundMember.Bind(compiler, value)
                : throw new NotImplementedException();
    }
}
