using ZSharp.Platform.Runtime;

using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A class while it is being loaded. Its own code and that of the classes loading
    /// with it reach it through this, as there is no type to reflect on yet.
    /// </summary>
    internal sealed class ClassInProgress(ILLoader loader, Package.DotNet.Class node, ClassDefinition definition)
        : CompilerObject
        , ICompileIRType
        , ICTCallable
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , IDefinedClass
        , IAssignableTo
        , ICompileIRExpression
        , ITyped
    {
        public Package.DotNet.Class Node { get; } = node;

        /// <summary>
        /// A class used as a value is its type object, even while it is being defined.
        /// </summary>
        CompilerObject ITyped.Type => loader.Class(typeof(Type));

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.Call(loader.TypeOf(Definition), []));

        /// <summary>
        /// A class still loading goes to itself, to the interfaces it implements, and
        /// wherever its base goes.
        /// </summary>
        public bool IsAssignableTo(CompilerObject target)
            => target.Is<IDefinedClass>(out var defined) && defined.Node == Node
            || node.Interfaces.Any(@interface => Scope.HIRLoader.LoadType(@interface).Ok(out var loaded) && Scope.TS.IsAssignableTo(target, loaded))
            || Base is { } @base && Scope.TS.IsAssignableTo(target, @base);

        public ClassDefinition Definition { get; } = definition;

        /// <summary>
        /// A class with no constructor gets one here, as loading doesn't change HIR.
        /// </summary>
        public List<Package.DotNet.Constructor> Constructors { get; }
            = loader.IsInterface(node) ? [] : node.Constructors.Count == 0 ? [new() { Body = new() }] : node.Constructors;

        public Dictionary<Package.DotNet.Method, ClassMethod> Methods { get; } = [];

        /// <summary>
        /// The class this one derives from, or <see langword="null"/> if it can't be loaded.
        /// </summary>
        public CompilerObject? Base
            => node.Base is null
                ? loader.Class(typeof(object))
                : Scope.HIRLoader.LoadType(node.Base).Ok(out var loaded) ? loaded : null;

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => target is Runtime
                ? Result<LIR.Type>.Ok(Definition)
                : Result<LIR.Type>.Error($"Class {Definition.Name} can only be compiled for the CLR runtime.");

        public IResult Call(Argument[] arguments)
            => Scope.CG.Call(
                OverloadGroup.Of(Constructors.Zip(Definition.Constructors).Select(constructor => new Callable(
                    $"A constructor of {Definition.Name}",
                    constructor.Second,
                    loader.CallParameters(constructor.First.Parameters, constructor.First.CustomMetadata),
                    null,
                    () => this,
                    loader
                ))),
                arguments
            );

        public IResult Member(MemberName member)
            => Find(member, null);

        public IResult Member(CompilerObject @object, MemberName member)
            => Find(member, @object);

        private IResult Find(MemberName member, CompilerObject? receiver)
        {
            var methods = Methods
                .Where(method => method.Key.Name == member && method.Key.IsStatic == receiver is null)
                .ToArray();

            if (methods.Length > 0)
            {
                if (methods.Any(method => method.Key.GenericParameters.Count > 0))
                    return Result.Error($"Generic method {Definition.Name}.{member} can only be reached once its class is loaded.");

                return Result.Ok(OverloadGroup.Of(methods.Select(method => new Callable(
                    $"{Definition.Name}.{member}",
                    method.Value,
                    loader.CallParameters(method.Key.Parameters, method.Key.CustomMetadata),
                    receiver,
                    method.Key.ReturnType is { } returnType ? () => Scope.HIRLoader.LoadType(returnType).Unwrap() : null,
                    loader
                ))));
            }

            if (Node.Properties.FirstOrDefault(property => property.Name == member && property.IsStatic == receiver is null) is { } property)
                return Result.Ok(new Accessor(
                    receiver,
                    () => Scope.HIRLoader.LoadType(property.Type!).Unwrap(),
                    property.Getter is { } getter ? Methods[getter] : null,
                    property.Setter is { } setter ? Methods[setter] : null
                ));

            if (receiver is not null && Definition.Fields.FirstOrDefault(field => field.Name == member) is { } field)
            {
                var type = Node.Fields.First(item => item.Name == member).Type;

                return Result.Ok(new Accessor(
                    receiver,
                    () => Scope.HIRLoader.LoadType(type).Unwrap(),
                    Access(field, receiver, isSet: false),
                    Access(field, receiver, isSet: true)
                ));
            }

            if (Base is not { } @base)
                return Result.Error(new UnsupportedError($"{Definition.Name} has no member named {member}."));

            return receiver is null
                ? Scope.CG.Member(@base, member)
                : Scope.CG.Member(new Upcast(receiver, @base), member);
        }

        private ClassFieldAccess Access(ClassField field, CompilerObject receiver, bool isSet)
        {
            var signature = new LIR.Signature();

            signature.Parameters.Add(new(Definition));

            if (isSet)
                signature.Parameters.Add(new(field.Type));
            else
                signature.ReturnType = field.Type;

            return new(field, isSet, signature);
        }
    }

    /// <summary>
    /// A method or constructor of a class being loaded, optionally bound to the object it
    /// is called on. A method goes where a delegate is expected, as any CLR method does.
    /// </summary>
    internal sealed class Callable(
        string name,
        LIR.ICallable callable,
        List<HIR.Code.Parameter> parameters,
        CompilerObject? receiver,
        Func<CompilerObject>? returnType,
        ILLoader loader
    )
        : CompilerObject
        , ICTCallable
        , ISpecific
        , ICTImplicitCastTo
        , ICTCastTo
    {
        /// <summary>
        /// A delegate of a type whose parameters go to the method's, and whose result is
        /// where what the method returns goes.
        /// </summary>
        public IResult ImplicitCast(CompilerObject type)
        {
            if (callable is not ClassMethod method
                || !type.Is<IILType>(out var known) || known.GetILType() is not { } delegateType
                || !typeof(Delegate).IsAssignableFrom(delegateType) || delegateType.GetMethod("Invoke") is not { } invoke
                || Arguments.Types(parameters) is not { } types || !Fits(types, invoke))
                return Result.Error(new UnsupportedError($"{name} isn't a delegate of that type."));

            return Result.Ok(new BoundDelegate(loader, loader.Delegate(delegateType, method), receiver, type));
        }

        public IResult Cast(CompilerObject type)
            => ImplicitCast(type);

        private bool Fits(CompilerObject[] types, System.Reflection.MethodInfo invoke)
        {
            var accepted = invoke.GetParameters();

            if (types.Length != accepted.Length
                || !types.Zip(accepted).All(pair => Scope.TS.IsAssignableTo(pair.First, loader.Class(pair.Second.ParameterType))))
                return false;

            return invoke.ReturnType == typeof(void)
                ? returnType is null
                : returnType is not null && Scope.TS.IsAssignableTo(loader.Class(invoke.ReturnType), returnType());
        }

        public IResult Call(Argument[] arguments)
        {
            if (Arguments.Parameters(parameters) is not { } matching || Arguments.Match(matching, arguments) is not { } values)
                return Result.Error($"{name} can't be called with these arguments.");

            return Result.Ok(new Call(callable, [.. (receiver is null ? [] : new[] { receiver }), .. values], returnType));
        }

        public bool IsAsSpecificAs(CompilerObject other)
            => Arguments.Types(parameters) is { } types && ZSharp.Compiler.Parameters.AreAsSpecificAs(types, other);
    }

    /// <summary>
    /// A delegate made of a method, bound to the object it was reached through, if any.
    /// </summary>
    internal sealed class BoundDelegate(ILLoader loader, LIR.FunctionHandle make, CompilerObject? receiver, CompilerObject type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            LIR.Expression bound = new LIR.Constant(loader.Handle(typeof(object)), null);

            if (receiver is not null)
                if (Scope.IR.CompileExpression(receiver, target).When(out var compiled).Error(out var error))
                    return Result<LIR.Expression>.Error(error);
                else
                    bound = compiled!;

            return Result<LIR.Expression>.Ok(new LIR.Call(make, [bound]));
        }
    }
}
