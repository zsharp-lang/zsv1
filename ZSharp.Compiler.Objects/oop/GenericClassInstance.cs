using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class GenericClassInstance(
        GenericClass origin,
        Mapping<GenericParameter, CompilerObject> genericArguments
    )
        : CompilerObject
        , ICTGetMember<MemberName>
        , ICTCallable
        , IReference
        , ICompileIRType<IR.ConstructedClass>
    {
        CompilerObject IReference.Origin => Origin;

        public ReferenceContext? Context { get; set; } = null;

        public GenericClass Origin { get; set; } = origin;

        public Mapping<GenericParameter, CompilerObject> Arguments { get; set; } = genericArguments;

        public Mapping<MemberName, CompilerObject> Members { get; set; } = [];

        public CompilerObject Member(Compiler.Compiler compiler, string member)
        {
            if (Members.ContainsKey(member)) return Members[member];

            var origin = compiler.Member(Origin, member);

            if (Arguments.Count != 0)
                //origin = compiler.CreateReference(origin, Context);
                throw new NotImplementedException();

            return Members[member] = origin;
        }

        IR.ConstructedClass ICompileIRType<IR.ConstructedClass>.CompileIRType(Compiler.Compiler compiler)
        {
            var result = new IR.ConstructedClass(
                compiler.CompileIRObject<IR.Class, IR.Module>(Origin, null)
            );

            foreach (var parameter in Origin.GenericParameters)
                result.Arguments.Add(compiler.CompileIRType(Arguments[parameter]));

            return result;
        }

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            CompilerObject? constructor = null;

            if (Origin.GenericParameters.Count == 0)
                constructor = Origin.Constructor;

            if (constructor is null)
                throw new NotImplementedException();

            return compiler.Call(constructor, arguments);
        }
    }
}
