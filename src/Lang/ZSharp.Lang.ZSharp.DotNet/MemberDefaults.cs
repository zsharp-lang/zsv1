using ZSharp.Compiler;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The accessibility a container of the CLR's gives its members unless they say their
    /// own: what holds them decides, as a class's members are private, an interface's
    /// public, and a module's functions public and its globals private. A container
    /// compiles its members in it.
    /// </summary>
    public sealed class MemberDefaults(Context? parent, Package.DotNet.Accessibility methods, Package.DotNet.Accessibility fields)
        : ChildContext(parent)
    {
        public Package.DotNet.Accessibility Methods { get; } = methods;

        public Package.DotNet.Accessibility Fields { get; } = fields;

        /// <summary>
        /// What a method or a constructor is unless it says otherwise.
        /// </summary>
        public static Package.DotNet.Accessibility MethodsIn(ASTCompiler compiler)
            => compiler.Contexts.Has<MemberDefaults>(out var defaults) ? defaults.Methods : Package.DotNet.Accessibility.Public;

        /// <summary>
        /// What a field is unless it says otherwise.
        /// </summary>
        public static Package.DotNet.Accessibility FieldsIn(ASTCompiler compiler)
            => compiler.Contexts.Has<MemberDefaults>(out var defaults) ? defaults.Fields : Package.DotNet.Accessibility.Private;

        /// <summary>
        /// Compiles a container's members with the defaults it gives them.
        /// </summary>
        public static T Within<T>(ASTCompiler compiler, Package.DotNet.Accessibility methods, Package.DotNet.Accessibility fields, Func<T> compile)
            => compiler.Contexts.Within(new MemberDefaults(compiler.Contexts.Current, methods, fields), compile);
    }
}
