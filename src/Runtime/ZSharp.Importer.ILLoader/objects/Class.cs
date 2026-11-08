using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A CLR type. It discovers members when asked, so loading a type never reaches
    /// the types its members mention.
    /// </summary>
    internal sealed class Class(ILLoader loader, Type il)
        : CompilerObject
        , ICompileIRType
        , ICTCallable
        , ICTCastTo
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , IRTCallable
        , ICTGetIndex
        , IRTImplicitCastTo
        , IRTGetIndex
        , IRTSetIndex
        , IRTCastTo
        , IRTTypeMatch
        , IILType
        , IAssignableTo
        , ICompileIRExpression
        , ITyped
    {
        public Type IL { get; } = il;

        /// <summary>
        /// A class used as a value is its type object.
        /// </summary>
        CompilerObject ITyped.Type => loader.Class(typeof(Type));

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.Call(loader.TypeOf(loader.Handle(IL)), []));

        public Type GetILType() => IL;

        /// <summary>
        /// A CLR type only goes to other CLR types.
        /// </summary>
        public bool IsAssignableTo(CompilerObject target)
            => target.Is<IILType>(out var type) && type.GetILType().IsAssignableFrom(IL);

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => Result<LIR.Type>.Ok(loader.Handle(IL));

        /// <summary>
        /// Calling a type constructs an instance of it. An array is made by indexing the
        /// type of its elements with its length instead, though the CLR gives array types
        /// constructors.
        /// </summary>
        public IResult Call(Argument[] arguments)
            => IL.IsArray
                ? Result.Error($"An array of {IL.GetElementType()!.Name} is made by giving {IL.GetElementType()!.Name} its length, as in {IL.GetElementType()!.Name}[4].")
                : IL.IsValueType && arguments.Length == 0 && IL.GetConstructor(Type.EmptyTypes) is null
                ? Result.Ok(new Literal(null, IL, loader))
                : Core.Compiler.ModuleScope.CG.Call(
                OverloadGroup.Of(IL.GetConstructors().Select(constructor => new Method(loader, constructor, null))),
                arguments
            );

        /// <summary>
        /// Casting a value converts it as C# casts it: as it would implicitly, or with a
        /// numeric conversion that can lose information, or with what a type defines as an
        /// explicit conversion.
        /// </summary>
        public IResult Cast(CompilerObject @object, CompilerObject type)
            => Converted(@object, type, implicitly: false)
            ?? Result.Error(new UnsupportedError($"{IL.Name} has no conversion to that type."));

        /// <summary>
        /// A conversion of a value of this type to another, as C# has it: numeric ones, and
        /// those either type defines. A cast also has the conversions that can lose a value.
        /// </summary>
        private IResult? Converted(CompilerObject @object, CompilerObject type, bool implicitly)
        {
            // A cast also goes down to a type that isn't a CLR type yet, as a class being
            // loaded with the code, if that type goes to this one.
            if (!implicitly && !IL.IsValueType && !type.Is<IILType>(out _)
                && Core.Compiler.ModuleScope.TS.IsAssignableTo(this, type)
                && Core.Compiler.ModuleScope.IR.CompileType(type, loader.Runtime).Ok(out var lowered))
                return Result.Ok(new Call(loader.Downcast(IL, lowered), [@object], () => type));

            if (!type.Is<IILType>(out var known) || known.GetILType() is not { } to || to == IL)
                return null;

            if (Conversions.IsNumeric(IL) && Conversions.IsNumeric(to) && (!implicitly || Conversions.Widens(IL, to)))
                return Result.Ok(new Call(loader.Convert(IL, to), [@object], () => type));

            // A cast also goes down, as in C#: to a type that derives from this one, or
            // between an interface and a type that isn't sealed, which might implement it.
            if (!implicitly && !IL.IsValueType && (to.IsAssignableTo(IL) || (to.IsInterface && !IL.IsSealed) || (IL.IsInterface && !to.IsSealed)))
                return Result.Ok(new Call(loader.Downcast(IL, to), [@object], () => type));

            var defined = Conversions.Defined(IL, to, "op_Implicit") ?? (implicitly ? null : Conversions.Defined(IL, to, "op_Explicit"));

            return defined is null ? null : Core.Compiler.ModuleScope.CG.Call(new Method(loader, defined, null), [new(@object)]);
        }

        /// <summary>
        /// Whether a value of this type is of another type, as C# tells it with <c>is</c>.
        /// A value type is known to be of a type or not; a reference is checked as the
        /// code runs, and <c>null</c> is of no type.
        /// </summary>
        public IResult<MatchResult, Error> Match(CompilerObject value, CompilerObject pattern)
        {
            if (!pattern.Is<IILType>(out var known) || known.GetILType() is not { } type)
                return Result<MatchResult>.Error(new UnsupportedError($"A value of {IL.Name} can only be matched against a CLR type."));

            CompilerObject match = IL.IsValueType
                ? new Literal(type.IsAssignableFrom(IL), typeof(bool), loader)
                : new Call(loader.IsOf(IL, type), [value], () => loader.Class(typeof(bool)));

            return Result<MatchResult>.Ok(new MatchResult { Match = match });
        }

        /// <summary>
        /// Indexing a value calls its indexers, as in C#: their getters and setters are its
        /// methods, so overloading picks between indexers as between any methods.
        /// </summary>
        public IResult Index(CompilerObject @object, Argument[] arguments)
        {
            if (IL.IsArray)
                return Element(@object, arguments);

            var indexers = Indexers();

            if (indexers.Length == 0)
                return Result.Error(new UnsupportedError($"{IL.Name} has no indexer."));

            var getters = indexers.Where(indexer => indexer.GetMethod is { IsPublic: true }).Select(indexer => (CompilerObject)new Method(loader, indexer.GetMethod!, @object)).ToArray();

            if (getters.Length == 0)
                return Result.Error($"The indexer of {IL.Name} can't be read.");

            if (Core.Compiler.ModuleScope.CG.Call(OverloadGroup.Of(getters), arguments).When(out var read).Error(out var error))
                return Result.Error(error);

            return Result.Ok(new Indexed(read!, value => Index(@object, arguments, value)));
        }

        public IResult Index(CompilerObject @object, Argument[] arguments, CompilerObject value)
        {
            if (IL.IsArray)
                return Element(@object, arguments) is var element && element.Ok(out var indexed)
                    ? Core.Compiler.ModuleScope.CG.Set(indexed, value)
                    : element;

            var setters = Indexers()
                .Where(indexer => indexer.SetMethod is { IsPublic: true })
                .Select(indexer => (CompilerObject)new Method(loader, indexer.SetMethod!, @object))
                .ToArray();

            return setters.Length == 0
                ? Result.Error($"The indexer of {IL.Name} can't be assigned.")
                : Core.Compiler.ModuleScope.CG.Call(OverloadGroup.Of(setters), [.. arguments, new Argument(value)]);
        }

        /// <summary>
        /// An element of an array, read and written by the <c>Get</c> and <c>Set</c> methods
        /// the CLR gives every array type, so its indices are matched as a call's arguments
        /// are, and an array of several dimensions takes one for each.
        /// </summary>
        private IResult Element(CompilerObject array, Argument[] arguments)
        {
            if (Core.Compiler.ModuleScope.CG.Call(new Method(loader, IL.GetMethod("Get")!, array), arguments).When(out var read).Error(out var error))
                return Result.Error(error);

            return Result.Ok(new Indexed(read!, value =>
                Core.Compiler.ModuleScope.CG.Call(new Method(loader, IL.GetMethod("Set")!, array), [.. arguments, new Argument(value)])));
        }

        private PropertyInfo[] Indexers()
            => [.. IL.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.GetIndexParameters().Length > 0)];

        /// <summary>
        /// Giving a type values, as in <c>Int32[4]</c>, makes an array of that length, with
        /// a dimension for each value, as in <c>Int32[2, 3]</c>, through the constructor the
        /// CLR gives the array type, so the values are matched as a call's arguments are.
        /// Each element is the default value of the type.
        /// </summary>
        private IResult NewArray(Argument[] arguments)
        {
            if (IL.ContainsGenericParameters)
                return Result.Error($"{IL.Name} takes type arguments, not lengths.");

            var array = arguments.Length == 1 ? IL.MakeArrayType() : IL.MakeArrayType(arguments.Length);

            return Core.Compiler.ModuleScope.CG.Call(
                OverloadGroup.Of(array.GetConstructors().Select(constructor => new Method(loader, constructor, null))),
                arguments
            );
        }

        /// <summary>
        /// Giving a generic type its arguments, as in <c>List[Int32]</c>, instantiates it.
        /// A type given nothing, as in <c>Int32[]</c>, is the type of its arrays, as C#
        /// writes it.
        /// </summary>
        public IResult Index(Argument[] arguments)
        {
            if (arguments.Length == 0)
                return Result.Ok(loader.Class(IL.MakeArrayType()));

            if (loader.Types(arguments, IL.IsGenericTypeDefinition ? IL.GetGenericArguments() : []) is not { } types)
                return NewArray(arguments);

            if (!IL.IsGenericTypeDefinition)
                return Result.Error(new UnsupportedError($"{IL.Name} is not generic."));

            if (types.Length != IL.GetGenericArguments().Length)
                return Result.Error($"{IL.Name} takes {IL.GetGenericArguments().Length} type arguments, not {types.Length}.");

            try
            {
                return Result.Ok(loader.Class(IL.MakeGenericType(types)));
            }
            catch (ArgumentException exception)
            {
                return Result.Error(exception.Message);
            }
        }

        /// <summary>
        /// A value of a delegate type is called through its <c>Invoke</c> method, as in C#.
        /// </summary>
        public IResult Call(CompilerObject @object, Argument[] arguments)
            => typeof(Delegate).IsAssignableFrom(IL) && IL.GetMethod("Invoke") is { } invoke
                ? new Method(loader, invoke, @object).Call(arguments)
                : Result.Error(new UnsupportedError($"A value of {IL.Name} can't be called."));

        /// <summary>
        /// Casting a type to what describes it is reflection.
        /// </summary>
        public IResult Cast(CompilerObject targetType)
        {
            var node = loader.Load(IL);

            return Core.Compiler.ModuleScope.TS.IsAssignableTo(targetType, loader.Class(node.GetType()))
                ? Result.Ok(new Literal(node, node.GetType(), loader))
                : Result.Error(new UnsupportedError($"{IL.Name} can't be cast to that type."));
        }

        /// <summary>
        /// A value of a value type becomes an object by boxing it, when it goes to a type
        /// it is assignable to and that isn't its own. Anything else that is assignable
        /// stays as it is.
        /// </summary>
        public IResult ImplicitCast(CompilerObject @object, CompilerObject type)
            => Converted(@object, type, implicitly: true) is { } converted
                ? converted
                : IL.IsValueType
            && Core.Compiler.ModuleScope.TS.IsAssignableTo(type, this)
            && !Core.Compiler.ModuleScope.TS.IsAssignableTo(this, type)
                ? Result.Ok(new Call(loader.Box(IL), [@object], () => type))
                : Result.Error(new UnsupportedError($"{IL.Name} has no conversion to that type."));

        public IResult Member(MemberName member)
            => Find(BindingFlags.Static, member, null);

        public IResult Member(CompilerObject @object, MemberName member)
            => Find(BindingFlags.Instance, member, @object);

        private IResult Find(BindingFlags kind, MemberName member, CompilerObject? receiver)
        {
            var flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | kind;

            var methods = IL
                .GetMethods(flags)
                .Where(method => !method.IsSpecialName && Named(method, member))
                .ToArray<MethodBase>();

            if (methods.Length > 0)
                return Result.Ok(OverloadGroup.Of(methods.Select(method => new Method(loader, method, receiver))));

            if (IL.GetProperties(flags).FirstOrDefault(property => Named(property, member)) is { } found)
                return Result.Ok(new Accessor(
                    receiver,
                    () => loader.Class(found.PropertyType),
                    found.GetMethod is { IsPublic: true } getter ? loader.Handle(getter) : null,
                    found.SetMethod is { IsPublic: true } setter ? loader.Handle(setter) : null
                ));

            if (IL.GetEvents(flags).FirstOrDefault(@event => Named(@event, member)) is { } @event)
                return Result.Ok(new Event(loader, @event, receiver));

            if (IL.GetFields(flags).FirstOrDefault(field => Named(field, member)) is { } field)
                // A constant, as an enum's member is, has no storage: it is its value.
                return field.IsLiteral
                    ? Result.Ok(new Literal(field.GetRawConstantValue(), field.FieldType, loader))
                    : Result.Ok(new Accessor(
                        receiver,
                        () => loader.Class(field.FieldType),
                        loader.Handle(field, isSet: false),
                        field.IsInitOnly ? null : loader.Handle(field, isSet: true),
                        field.IsInitOnly || (!field.IsStatic && IL.IsValueType) ? null : loader.FieldAddress(field)
                    ));

            if (receiver is null && IL.GetNestedType(member, BindingFlags.Public) is { } nested)
                return Result.Ok(loader.Class(nested));

            return Result.Error(new UnsupportedError($"{IL.Name} has no member named {member}."));
        }

        private static bool Named(MemberInfo member, string name)
            => (member.GetCustomAttribute<AliasAttribute>()?.Name ?? member.Name) == name;
    }
}
