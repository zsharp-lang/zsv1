using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// Makes the CLR meta-types the defaults for <c>class</c> and <c>module</c>.
    /// </summary>
    public static class DotNetMetaTypes
    {
        public static MetaTypes Install(
            Language language,
            MappedASTCompiler mapped,
            MappedBinder binding,
            HIRRunner runner,
            ILLoader il,
            out Bodies bodies
        )
        {
            var metaTypes = new MetaTypes(language, of => Resolve(of, runner, il));
            var container = new ContainerCompiler(language);

            var localFunction = il.Load(typeof(Package.DotNet.LocalFunction));
            var method = il.Load(typeof(Package.DotNet.Method));
            var body = new CodeBody(
                language,
                il,
                (compiler, node) => metaTypes.Compile(compiler, node, localFunction),
                (compiler, node) => metaTypes.Compile(compiler, node, method),
                expression => runner.EvaluateExpression(expression)
            );
            bodies = body.Bodies;

            var @class = il.Load(typeof(Package.DotNet.Class));
            var module = il.Load(typeof(Package.DotNet.Module));
            var constructor = il.Load(typeof(Package.DotNet.Constructor));

            var classes = new ClassMetaType(language, il, container, body, @class);
            var @interface = il.Load(typeof(Package.DotNet.Interface));

            metaTypes.Register(@class, classes.Compile);
            metaTypes.Register(@interface, new InterfaceMetaType(il, container, body, classes, @interface).Compile);
            metaTypes.Register(module, new ModuleMetaType(il, body, container, module).Compile);
            metaTypes.Register(method, new MethodMetaType(il, body, method).Compile);
            metaTypes.Register(constructor, new ConstructorMetaType(il, body, constructor).Compile);
            metaTypes.Register(localFunction, new LocalFunctionMetaType(language, il, body, localFunction).Compile);

            metaTypes.Default<AST.TypeDefinition>(@class);
            metaTypes.Default<AST.Module>(module);
            metaTypes.Default<AST.Function>(method);
            metaTypes.Default<AST.Constructor>(constructor);

            metaTypes.Install(mapped);

            // A name bound to a local's definition can be bound to another one, which is how
            // a let reuses a name. The type of what holds the definition decides.
            var local = il.Load(typeof(Code.Local));

            binding.Merges.Add((existing, value) =>
                existing is Code.Get { Instance: Code.Local { Type: var before } } && ReferenceEquals(before, local)
                && value is Code.Get { Instance: Code.Local { Type: var after } } && ReferenceEquals(after, local)
                    ? value
                    : null
            );

            // A capture of a local or a parameter is bound over it, so the code around a
            // function that captures it refers to the capture from then on.
            var capture = il.Load(typeof(Code.Capture));
            HIR.Instance[] capturable = [local, il.Load(typeof(Package.DotNet.Parameter)), il.Load(typeof(Code.Parameter))];

            binding.Merges.Add((existing, value) =>
                existing is Code.Get { Instance: Code.Local { Type: var before } } && capturable.Any(kind => ReferenceEquals(before, kind))
                && value is Code.Get { Instance: Code.Local { Type: var after } } && ReferenceEquals(after, capture)
                    ? value
                    : null
            );

            binding.Merges.Add((existing, value) =>
                Overloads(existing, method) is { } before && Overloads(value, method) is { } after
                    ? Joined(existing, before, after)
                    : null
            );

            // Definitions of a kind that is overloadable, as Z# functions are, overload by
            // their name, as methods do: what the kind is, its node's type says.
            binding.Merges.Add((existing, value) =>
                Overloadables(existing, il) is { } before && Overloadables(value, il) is { } after
                    ? Joined(existing, before, after)
                    : null
            );

            // Inside a definition of an overloadable kind, its own name is the overloads it
            // is one of, as a method's is.
            binding.Shadows.Add((shadowed, value) =>
                Overloadables(shadowed, il) is { } before && Overloadables(value, il) is { } after
                    ? Joined(shadowed, before, after)
                    : null
            );

            // A method hides the methods of its name outside it by overloading them, as a
            // method's own name, inside it, is the overloads it is one of.
            binding.Shadows.Add((shadowed, value) =>
                Overloads(shadowed, method) is { } before && Overloads(value, method) is { } after
                    ? Joined(shadowed, before, after)
                    : null
            );

            return metaTypes;
        }

        /// <summary>
        /// The methods a name stands for: one being built, one already defined, or a
        /// group of them. Anything else doesn't overload.
        /// </summary>
        private static List<HIR.Instance>? Overloads(Code.Expression value, HIR.Instance method)
            => value switch
            {
                Code.Get { Instance: Code.OverloadGroup group } => group.Items,
                Code.Get { Instance: Code.Local local } when ReferenceEquals(local.Type, method) => [local],
                Code.Get { Instance: Code.Reference { Value: Package.DotNet.Method } reference } => [reference],
                Code.Get { Instance: Package.DotNet.Method defined } => [defined],
                _ => null
            };

        /// <summary>
        /// Overloads joined, each once: what a name already stands for, if it has them all.
        /// </summary>
        /// <summary>
        /// The definitions a name stands for that are of an overloadable kind: one being
        /// built, whose local's type is its kind's description, one already defined, or a
        /// group of them.
        /// </summary>
        private static List<HIR.Instance>? Overloadables(Code.Expression value, ILLoader il)
            => value switch
            {
                Code.Get { Instance: Code.OverloadGroup group } when group.Items.All(item => Overloadable(item, il)) => group.Items,
                Code.Get { Instance: Code.Local local } when Overloadable(local, il) => [local],
                Code.Get { Instance: Code.Reference reference } when Overloadable(reference, il) => [reference],
                Code.Get { Instance: var defined } when defined is HIR.Overloadable => [defined],
                _ => null
            };

        private static bool Overloadable(HIR.Instance item, ILLoader il)
            => item switch
            {
                HIR.Overloadable => true,
                Code.Local { Type: HIR.Instance kind } => il.Described(kind) is Type type && typeof(HIR.Overloadable).IsAssignableFrom(type),
                Code.Reference { Value: HIR.Overloadable } => true,
                Code.Reference { Value: null, Kind: { } kind } => il.Described(kind) is Type type && typeof(HIR.Overloadable).IsAssignableFrom(type),
                _ => false,
            };

        private static Code.Expression Joined(Code.Expression existing, List<HIR.Instance> before, List<HIR.Instance> after)
        {
            var added = after.Where(item => !before.Any(known => ReferenceEquals(known, item))).ToList();

            return added.Count == 0 ? existing : new Code.Get(Group([.. before, .. added]));
        }

        private static Code.OverloadGroup Group(List<HIR.Instance> items)
        {
            var group = new Code.OverloadGroup();

            group.Items.AddRange(items);

            return group;
        }

        /// <summary>
        /// An <c>of</c> runs when the definition is compiled, when the definitions of
        /// the same module are only code, so they can't be meta-types.
        /// </summary>
        private static IResult<HIR.Instance, Error> Resolve(Code.Expression of, HIRRunner runner, ILLoader il)
        {
            if (of is Code.Get { Instance: Code.Local local })
                return Result<HIR.Instance>.Error($"{local.Name} can't be a meta-type in its own module.");

            var described = new Code.Cast { Value = of, Type = new Code.Get(il.Load(typeof(HIR.Instance))) };

            if (runner.EvaluateExpression(described).When(out var value).Error(out var error))
                return Result<HIR.Instance>.Error(error);

            return value is HIR.Instance instance
                ? Result<HIR.Instance>.Ok(instance)
                : Result<HIR.Instance>.Error($"A meta-type must be an HIR instance, but got {value?.GetType().Name ?? "null"}.");
        }
    }
}
