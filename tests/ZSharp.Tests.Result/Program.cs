using CommonZ;

IResult<string?, Error> nullOk = Result<string?>.Ok(null);
Console.WriteLine($"Ok(null) IsOk={nullOk.IsOk} IsError={nullOk.IsError} Unwrap={nullOk.Unwrap() is null}");

IResult<string?, Error> ok = Result<string?>.Ok("value");
Console.WriteLine($"Ok IsOk={ok.IsOk} Unwrap={ok.Unwrap()}");

IResult<string?, Error> failed = Result<string?>.Error("boom");
Console.WriteLine($"Error IsError={failed.IsError} message={failed.UnwrapError()}");

try { failed.Unwrap(); Console.WriteLine("Unwrap on error did NOT throw"); }
catch (InvalidOperationException e) { Console.WriteLine($"Unwrap on error throws: {e.Message}"); }

try { ok.UnwrapError(); Console.WriteLine("UnwrapError on ok did NOT throw"); }
catch (InvalidOperationException e) { Console.WriteLine($"UnwrapError on ok throws: {e.Message}"); }

IResult<string?, Error> many = Result<string?>.Error(new Error[] { new ErrorMessage("a"), new ErrorMessage("b") });
Console.WriteLine($"Aggregate is AggregateError={many.UnwrapError() is AggregateError}");

Console.WriteLine($"When maps ok={ok.When(v => v!.Length.ToString()).Unwrap()}");
Console.WriteLine($"When keeps error={failed.When(v => "x").UnwrapError()}");
Console.WriteLine($"Else maps error={failed.Else<string>(e => "mapped").UnwrapError()}");

IResult<string?, string> generic = Result<string?, string>.Ok(null);
Console.WriteLine($"two-parameter Ok(null) IsOk={generic.IsOk} IsError={generic.IsError}");

Console.WriteLine($"Ok(out) on Ok(null)={nullOk.Ok(out var nothing)} value-null={nothing is null}");
Console.WriteLine($"Ok(out) on value={ok.Ok(out var something)} value={something}");
Console.WriteLine($"Ok(out) on error={failed.Ok(out var none)}");
Console.WriteLine($"Error(out) on Ok(null)={nullOk.Error(out var noError)}");
