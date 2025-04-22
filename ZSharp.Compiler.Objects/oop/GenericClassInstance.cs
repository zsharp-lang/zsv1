using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class GenericClassInstance(
        GenericClass origin
    )
        : CompilerObject
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , ICTCallable
        , IReference
        , ICompileIRType<IR.ConstructedClass>
        , ICompileIRReference<IR.OOPTypeReference<IR.Class>>
        , ICompileIRReference<IR.OOPTypeReference>
        , IReferencable<GenericClassInstance>
    {
        CompilerObject IReference.Origin => Origin;

        public required ReferenceContext Context { get; set; }

        public GenericClass Origin { get; set; } = origin;

        public Mapping<MemberName, CompilerObject> Members { get; set; } = [];

        public CompilerObject Member(Compiler.Compiler compiler, string member)
        {
            if (Members.ContainsKey(member)) return Members[member];

            var origin = compiler.Member(Origin, member);

            if (Origin.GenericParameters.Count != 0)
                //origin = compiler.CreateReference(origin, Context);
                throw new NotImplementedException();

            return Members[member] = origin;
        }

        public CompilerObject Member(Compiler.Compiler compiler, CompilerObject instance, string member)
        {
            if (Members.ContainsKey(member)) return Members[member];

            var origin = compiler.Member(Origin, member);

            Members[member] = origin = compiler.Map(origin, @object => compiler.Feature<Referencing>().CreateReference(@object, Context));

            return compiler.Map(origin, @object => @object is IRTBoundMember bindable ? bindable.Bind(compiler, instance) : @object);
        }

        IR.ConstructedClass ICompileIRType<IR.ConstructedClass>.CompileIRType(Compiler.Compiler compiler)
        {
            var result = new IR.ConstructedClass(
                compiler.CompileIRObject<IR.Class, IR.Module>(Origin, null)
            );

            foreach (var parameter in Origin.GenericParameters)
                result.Arguments.Add(compiler.CompileIRType(Context.CompileTimeValues.Cache(parameter) ?? throw new()));

            return result;
        }

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            CompilerObject? constructor = null;

            //if (Origin.GenericParameters.Count == 0)
            constructor = Origin.Constructor;

            if (constructor is null)
                throw new NotImplementedException();

            var @ref = compiler.Feature<Referencing>();

            if (Context is not null)
                constructor = compiler.Map(constructor, @object => @ref.CreateReference(@object, Context));

            return compiler.Call(constructor, arguments);
        }

        IR.OOPTypeReference<IR.Class> ICompileIRReference<IR.OOPTypeReference<IR.Class>>.CompileIRReference(Compiler.Compiler compiler)
            => compiler.CompileIRType<IR.ConstructedClass>(this);

        GenericClassInstance IReferencable<GenericClassInstance>.CreateReference(Referencing @ref, ReferenceContext context)
        {
            Mapping<GenericParameter, CompilerObject> arguments = [];

            return new(Origin)
            {
                Context = context
            };
        }

        public override bool Equals(object? obj)
        {
            // TODO: instead of type == type, use assignable to
            if (obj is not GenericClassInstance other)
                return false;

            if (Origin != other.Origin)
                return false;

            foreach (var genericParameter in Origin.GenericParameters)
                if (Context[genericParameter] != other.Context[genericParameter])
                    return false;

            return true;
        }

        IR.OOPTypeReference ICompileIRReference<IR.OOPTypeReference>.CompileIRReference(Compiler.Compiler compiler)
            => compiler.CompileIRType<IR.ConstructedClass>(this);
    }
}
