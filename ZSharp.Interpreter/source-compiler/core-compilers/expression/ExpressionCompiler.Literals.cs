namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ExpressionCompiler
    {
        public ObjectResult Compile(ArrayLiteral array)
        {
            Objects.ArrayLiteral result = new();

            ObjectResult objectResult;
            foreach (var item in array.Items)
                if (
                    (objectResult = Compiler.CompileNode(item)).IsError
                )
                    return objectResult;
                else result.Items.Add(objectResult.Unwrap());

            return ObjectResult.Ok(result);
        }

        public ObjectResult Compile(LiteralExpression literal)
            => literal.Type switch
            {
                LiteralType.String => Compiler.Compiler.CreateString(literal.Value),
                LiteralType.False => Compiler.Compiler.CreateFalse(),
                LiteralType.True => Compiler.Compiler.CreateTrue(),
                LiteralType.Null => Compiler.Compiler.CreateNull(),
                LiteralType.Number => Compiler.Compiler.CreateInteger(int.Parse(literal.Value)),
                _ => null
            } is CompilerObject result
                ? ObjectResult.Ok(result)
                : Compiler.CompilationError($"Could not compile literal of type {literal.Type}", literal)
            ;
    }
}
