namespace ZSharp.Compiler
{
    /// <summary>
    /// What a meta-compiler produces for a definition. It is not HIR and does not
    /// outlive the handler that asked for it: the definition's expression keeps only
    /// <see cref="Build"/>, and only a cast looks at the rest.
    /// </summary>
    /// <param name="Build">Code that produces the definition object as HIR.</param>
    /// <param name="UnderlyingType">The HIR type of that object.</param>
    public sealed record CompiledDefinition(HIR.Code.Expression Build, HIR.Instance UnderlyingType);
}
