using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class GenericClassInstance
        : CompilerObject
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , ICTCallable
        , IReference
        , ICompileIRType<IR.ConstructedClass>
        , ICompileIRReference<IR.OOPTypeReference<IR.Class>>
        , ICompileIRReference<IR.OOPTypeReference>
        , IReferencable<GenericClassInstance>
        , IType
    {
        CompilerObject IReference.Origin => Origin;

        public ReferenceContext Context { get; set; }

        public GenericClass Origin { get; set; }

        public Mapping<GenericParameter, CompilerObject> GenericArguments { get; } = [];

        public Mapping<MemberName, CompilerObject> Members { get; set; } = [];

        #region Constructors

        public GenericClassInstance(GenericClass origin, ReferenceContext context)
        {
            Context = context;
            Origin = origin;

            foreach (var genricParameter in origin.GenericParameters)
                GenericArguments[genricParameter] = Context[genricParameter];
        }

        #endregion

        public CompilerObject Member(Compiler.Compiler compiler, string member)
        {
            if (Members.ContainsKey(member)) return Members[member];

            var origin = compiler.Member(Origin, member);

            if (Origin.GenericParameters.Count != 0)
                origin = compiler.Map(origin, @object => compiler.Feature<Referencing>().CreateReference(@object, Context));

            return Members[member] = origin;
        }

        public CompilerObject Member(Compiler.Compiler compiler, CompilerObject instance, string member)
        {
            if (Members.ContainsKey(member)) 
                return compiler.Map(Members[member], @object => @object is IRTBoundMember bindable ? bindable.Bind(compiler, instance) : @object);

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
                result.Arguments.Add(
                    compiler.CompileIRType(
                        Context.CompileTimeValues.Cache(GenericArguments[parameter]) ?? GenericArguments[parameter]
                    )
                );

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
            context = new(context)
            {
                Scope = this
            };

            foreach (var (key, value) in GenericArguments)
                context.CompileTimeValues.Cache(key, context.CompileTimeValues.Cache(value) ?? value);

            return new(Origin, context);
        }

        bool IType.IsEqualTo(Compiler.Compiler compiler, IType type)
        {
            if (type is not GenericClassInstance other)
                return false;

            if (Origin != other.Origin)
                return false;

            foreach (var genericParameter in Origin.GenericParameters)
                if (GenericArguments[genericParameter] is not IType thisGenericArgument)
                    throw new Compiler.InvalidCastException(GenericArguments[genericParameter], compiler.TypeSystem.Type);
                else if (other.GenericArguments[genericParameter] is not IType otherGenericArgument)
                    throw new Compiler.InvalidCastException(other.GenericArguments[genericParameter], compiler.TypeSystem.Type);
                else if (!compiler.TypeSystem.AreEqual(thisGenericArgument, otherGenericArgument))
                    return false;

            return true;
        }

        IR.OOPTypeReference ICompileIRReference<IR.OOPTypeReference>.CompileIRReference(Compiler.Compiler compiler)
            => compiler.CompileIRType<IR.ConstructedClass>(this);
    }
}
