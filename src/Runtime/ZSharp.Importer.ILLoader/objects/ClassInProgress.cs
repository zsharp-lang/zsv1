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
        /// A class still loading goes to itself, and wherever its base goes.
        /// </summary>
        public bool IsAssignableTo(CompilerObject target)
            => target.Is<IDefinedClass>(out var defined) && defined.Node == Node
            || Base is { } @base && Scope.TS.IsAssignableTo(target, @base);

        public ClassDefinition Definition { get; } = definition;

        /// <summary>
        /// A class with no constructor gets one here, as loading doesn't change HIR.
        /// </summary>
        public List<Package.DotNet.Constructor> Constructors { get; }
            = node.Constructors.Count == 0 ? [new() { Body = new() }] : node.Constructors;

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
        {
            var matching = Constructors
                .Zip(Definition.Constructors)
                .Select(constructor => (constructor.Second, Values: Arguments.Convert(
                    loader.CallParameters(constructor.First.Parameters, constructor.First.CustomMetadata),
                    arguments
                )))
                .Where(constructor => constructor.Values is not null)
                .ToArray();

            if (matching is not [var (constructor, values)])
                return Result.Error($"Class {Definition.Name} has no single constructor for {arguments.Length} arguments.");

            return Result.Ok(new Call(constructor, values!, () => this));
        }

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
                return Result.Ok(new ClassMethods(loader, Definition.Name, methods, receiver));

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
    /// The methods of a class being loaded that a name stands for, optionally bound to
    /// the object they are called on.
    /// </summary>
    internal sealed class ClassMethods(
        ILLoader loader,
        string @class,
        KeyValuePair<Package.DotNet.Method, ClassMethod>[] candidates,
        CompilerObject? receiver
    )
        : CompilerObject
        , ICTCallable
    {
        public IResult Call(Argument[] arguments)
        {
            var matching = candidates
                .Select(candidate => (Method: candidate, Values: Arguments.Convert(
                    loader.CallParameters(candidate.Key.Parameters, candidate.Key.CustomMetadata),
                    arguments
                )))
                .Where(candidate => candidate.Values is not null)
                .ToArray();

            if (matching is not [var (method, values)])
                return Result.Error($"{@class}.{candidates[0].Key.Name} has no single overload for {arguments.Length} arguments.");

            var returnType = method.Key.ReturnType;

            return Result.Ok(new Call(
                method.Value,
                [.. (receiver is null ? [] : new[] { receiver }), .. values!],
                returnType is null ? null : () => Scope.HIRLoader.LoadType(returnType).Unwrap()
            ));
        }
    }
}
