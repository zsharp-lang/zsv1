using ZSharp.Compiler;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The names of a CLR class's members, which the code of its methods and constructors
    /// reaches by name, as C# code reaches them: a field, a method, a property or a named
    /// constructor, written in the class. A class compiles its members in it. Only what is
    /// defined directly in the class has them, so a definition inside a method's body, as
    /// a module, doesn't.
    /// </summary>
    public sealed class ClassMembers(Context? parent, Context? owner, IReadOnlyList<string> names)
        : ChildContext(parent)
    {
        public IReadOnlyList<string> Names { get; } = names;

        /// <summary>
        /// The names of the members of the class that the definition being compiled is
        /// defined directly in, or none if it isn't in one.
        /// </summary>
        public static IReadOnlyList<string> Of(ASTCompiler compiler)
        {
            if (!compiler.Contexts.Has<ClassMembers>(out var members))
                return [];

            var definition = Bodies.DefinitionOf(compiler.Contexts.Current);
            var around = definition is ChildContext child ? Bodies.DefinitionOf(child.Parent) : null;

            return ReferenceEquals(around, members.owner) ? members.Names : [];
        }

        private readonly Context? owner = owner;

        /// <summary>
        /// Compiles a class's members with the names of the members it has.
        /// </summary>
        public static T Within<T>(ASTCompiler compiler, IEnumerable<AST.Statement> members, Func<T> compile)
        {
            var current = compiler.Contexts.Current;

            return compiler.Contexts.Within(new ClassMembers(current, Bodies.DefinitionOf(current), [.. NamesOf(members)]), compile);
        }

        /// <summary>
        /// The names a class's members are reached by: a <c>let</c>'s, a <c>fun</c>'s, an
        /// accessor's property's, and a named constructor's. A definition nested in the class
        /// is bound by its own name as the class compiles it.
        /// </summary>
        private static IEnumerable<string> NamesOf(IEnumerable<AST.Statement> members)
            => members
                .Select(statement => Language.Undecorate(statement, out _))
                .Select(statement => statement switch
                {
                    AST.ExpressionStatement { Expression: AST.LetExpression let } => let.Name,
                    AST.DefinitionStatement { Definition: AST.Function function } => function.Name,
                    AST.DefinitionStatement { Definition: AST.Constructor constructor } => constructor.Name,
                    _ => null
                })
                .OfType<string>()
                .Where(name => name.Length > 0)
                .Distinct();
    }
}
