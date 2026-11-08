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
        public static void Install(
            Language language,
            MappedASTCompiler mapped,
            MappedBinder binding,
            HIRRunner runner,
            ILLoader il
        )
        {
            var metaTypes = new MetaTypes(language, of => Resolve(of, runner, il));
            var container = new ContainerCompiler(language);

            var localFunction = il.Load(typeof(Package.DotNet.LocalFunction));
            var body = new CodeBody(language, il, (compiler, node) => metaTypes.Compile(compiler, node, localFunction));

            var @class = il.Load(typeof(Package.DotNet.Class));
            var module = il.Load(typeof(Package.DotNet.Module));
            var method = il.Load(typeof(Package.DotNet.Method));
            var constructor = il.Load(typeof(Package.DotNet.Constructor));

            metaTypes.Register(@class, new ClassMetaType(il, container, body, @class).Compile);
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

            // A capture of a local is bound over the local, so the code around a function
            // that captures it refers to the capture from then on.
            var capture = il.Load(typeof(Code.Capture));

            binding.Merges.Add((existing, value) =>
                existing is Code.Get { Instance: Code.Local { Type: var before } } && ReferenceEquals(before, local)
                && value is Code.Get { Instance: Code.Local { Type: var after } } && ReferenceEquals(after, capture)
                    ? value
                    : null
            );

            binding.Merges.Add((existing, value) =>
                Overloads(existing, method) is { } before && Overloads(value, method) is { } after
                    ? new Code.Get(Group([.. before, .. after]))
                    : null
            );
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
