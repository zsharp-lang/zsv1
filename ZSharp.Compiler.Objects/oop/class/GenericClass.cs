using CommonZ.Utils;
using System;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public class GenericClass
        : CompilerObject
        , ICTGetIndex
        , ICTGetMember_Old<MemberName>
        , IRTGetMember_Old<MemberName>
        , IReferencable<GenericClassInstance>
        , ICompileIRObject<IR.Class, IR.Module>
        , IEvaluable
        , IGenericInstantiable
    {
        [Flags]
        enum BuildState
        {
            None = 0,
            Base = 0b1,
            Interfaces = 0b10,
            Body = 0b100,
            Owner = 0b1000,
            Generic = 0b10000,
        }
        private readonly ObjectBuildState<BuildState> state = new();

        public IR.ConstructedClass? IR { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool Defined { init
            {
                if (value)
                {
                    foreach (var item in Enum.GetValues<BuildState>())
                        state[item] = true;
                }
            } 
        }

        public Collection<GenericParameter> GenericParameters { get; set; } = [];

        public IClass? Base { get; set; }

        public Mapping<CompilerObject, Implementation> Implementations { get; set; } = [];

        public CompilerObject Constructor { get; set; }

        public Collection<CompilerObject> Content { get; } = [];

        public Mapping<MemberName, CompilerObject> Members { get; } = [];

        public CompilerObject Member(Compiler.Compiler compiler, MemberName member)
            => Members[member];

        public GenericClassInstance CreateReference(Referencing @ref, ReferenceContext context)
        {
            int currentErrors = @ref.Compiler.Log.Logs.Count(l => l.Level == LogLevel.Error);

            foreach (var genericParameter in GenericParameters)
                if (!context.CompileTimeValues.Contains(genericParameter))
                    @ref.Compiler.Log.Error(
                        $"Missing generic argument for parameter {genericParameter.Name} in type {Name}",
                        this
                    );

            if (@ref.Compiler.Log.Logs.Count(l => l.Level == LogLevel.Error) > currentErrors)
                throw new(); // TODO: Huh???

            return new GenericClassInstance(this, new(context)
            {
                Scope = this
            });
        }

        CompilerObjectResult IGenericInstantiable.Instantiate(Compiler.Compiler compiler, Argument[] arguments)
        {
            var context = new ReferenceContext();

            foreach (var (genericParameter, genericArgument) in GenericParameters.Zip(arguments))
                context[genericParameter] = genericArgument.Object;

            return CompilerObjectResult.Ok(new GenericClassInstance(this, new(context)
            {
                Scope = this
            }));
        }

        CompilerObject ICTGetIndex.Index(Compiler.Compiler compiler, Argument[] index)
        {
            var context = new ReferenceContext();

            foreach (var (genericParameter, genericArgument) in GenericParameters.Zip(index))
                context[genericParameter] = genericArgument.Object;

            return compiler.Feature<Referencing>().CreateReference(this, context);
        }

        IR.Class ICompileIRObject<IR.Class, IR.Module>.CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            IR.Class @class = IR?.Class ?? new(Name);

            IR ??= new(@class);

            if (owner is not null && !state.Get(BuildState.Owner))
            {
                owner.Types.Add(@class!);

                state.Set(BuildState.Owner);
            }

            if (Base is not null && !state.Get(BuildState.Base))
            {
                state.Set(BuildState.Base);

                @class.Base = compiler.CompileIRType<IR.OOPTypeReference<IR.Class>>(Base);
            }

            if (Content.Count > 0 && !state.Get(BuildState.Body))
            {
                state.Set(BuildState.Body);

                foreach (var item in Content)
                    compiler.CompileIRObject(item, @class);
            }

            if (GenericParameters.Count > 0 && !state.Get(BuildState.Generic))
            {
                state.Set(BuildState.Generic);
                foreach (var parameter in GenericParameters)
                    @class.GenericParameters.Add(
                        compiler.CompileIRType<IR.GenericParameter>(parameter)
                    );
            }

            return @class;
        }

        CompilerObject IEvaluable.Evaluate(Compiler.Compiler compiler)
        {
            if (GenericParameters.Count != 0)
                return this;

            return new GenericClassInstance(this, new()
            {
                Scope = this
            });
        }

        CompilerObject IRTGetMember_Old<string>.Member(Compiler.Compiler compiler, CompilerObject value, string member)
        {
            if (GenericParameters.Count > 0)
                throw new InvalidOperationException();

            var memberObject = compiler.Member(this, member);

            if (memberObject is IRTBoundMember boundMember)
                return boundMember.Bind(compiler, value);

            if (memberObject is OverloadGroup group)
                return new OverloadGroup(group.Name)
                {
                    Overloads = [.. group.Overloads.Select(
                        overload => overload is IRTBoundMember boundMember ?
                        boundMember.Bind(compiler, value) : overload
                    )],
                };

            return memberObject;
        }
    }
}
