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
            ASTCompiler compiler,
            MappedASTCompiler mapped,
            HIRRunner runner,
            ILLoader il
        )
        {
            var metaTypes = new MetaTypes(language, compiler, of => Resolve(of, runner, il));
            var container = new ContainerCompiler(language, compiler, mapped);

            var body = new CodeBody(language, compiler, mapped, il);

            var @class = il.Load(typeof(Package.DotNet.Class));
            var module = il.Load(typeof(Package.DotNet.Module));
            var method = il.Load(typeof(Package.DotNet.Method));
            var constructor = il.Load(typeof(Package.DotNet.Constructor));

            metaTypes.Register(@class, new ClassMetaType(il, container, body, @class).Compile);
            metaTypes.Register(module, new ModuleMetaType(il, container, module).Compile);
            metaTypes.Register(method, new MethodMetaType(language, il, body, method).Compile);
            metaTypes.Register(constructor, new ConstructorMetaType(language, il, body, constructor).Compile);

            metaTypes.Default<AST.TypeDefinition>(@class);
            metaTypes.Default<AST.Module>(module);
            metaTypes.Default<AST.Function>(method);
            metaTypes.Default<AST.Constructor>(constructor);

            metaTypes.Install(mapped);
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
