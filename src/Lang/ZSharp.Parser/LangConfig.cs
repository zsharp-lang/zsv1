/// <summary>
/// The ZSHARP_MINIMAL_PARENTHESIS directive is used to use the minimal parenthesis syntax.
/// This syntax is very similar to the regular Z# syntax, but it ommits the parenthesis where possible.
/// Whenever a statement follows a place where a parenthesis is expected, the parser will parse the
/// <see cref="ZSharp.Parser.LangParser.Symbols.ThenDo"/> symbol.
/// </summary>
/// #define ZSHARP_MINIMAL_PARENTHESIS
