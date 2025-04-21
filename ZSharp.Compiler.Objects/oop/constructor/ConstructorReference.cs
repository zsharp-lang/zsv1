using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class ConstructorReference(Constructor origin, ReferenceContext context)
        : CompilerObject
        , ICTCallable
    {
        public Constructor Origin { get; } = origin;

        public ReferenceContext Context { get; } = context;

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            var result = compiler.Call(Origin, arguments);

            if (result is not RawCode rawCode)
                throw new();

            var owner = Origin.Owner;
            if (owner is null)
                throw new();

            var ownerReference = compiler.Feature<Referencing>().CreateReference(owner, Context);

            var invocationInstruction = rawCode.Code.Instructions.Last();
            if (invocationInstruction is IR.VM.CreateInstance createInstance)
            {
                var type = compiler.CompileIRReference<IR.OOPTypeReference<IR.Class>>(ownerReference);

                createInstance.Constructor = new IR.ConstructorReference(createInstance.Constructor.Member)
                {
                    OwningType = type
                };

                rawCode.Code.Types.Clear();
                rawCode.Code.Types.Add(ownerReference);
            }

            return rawCode;
        }
    }
}
