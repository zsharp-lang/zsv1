using ZSharp.Tokenizer;


using (StreamReader stream = File.OpenText("./tokenizerText.txt"))
    foreach (var token in Tokenizer.Tokenize(new(stream)))
    {
        if (token.Is(ZSharp.Text.TokenCategory.WhiteSpace))
            Console.WriteLine($"Token ({token.Type}) @ ({token.Span})");
        else
            Console.WriteLine($"Token ({token.Type}) @ ({token.Span}) = {token.Value}");
    }
