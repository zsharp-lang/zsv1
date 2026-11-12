// The entry point of Z#: the command line. It has the toolchain put together and runs a
// script. What gets compiled after that, and how, is up to the script.

// Diagnostics print to standard error, so a program's own output stays as it is.
//   --trace-reification  prints the code each reification receives and produces.
//   --dump-definitions   prints the HIR of each definition a document defines.
// What follows the script is the script's own, as `core:zsharp`'s `arguments`.
var script = Array.FindIndex(args, arg => !arg.StartsWith("--"));
var options = (script < 0 ? args : args[..script]).ToHashSet();
var files = script < 0 ? [] : new[] { args[script] };

if (files.Length != 1 || options.Except(["--trace-reification", "--dump-definitions"]).Any())
{
    Console.Error.WriteLine("Usage: zs [--trace-reification] [--dump-definitions] <file> [arguments...]");

    return 1;
}

Core.ZSharp.ModuleScope.Arguments = args[(script + 1)..];

var toolchain = new ZSharp.Toolchain.Toolchain(
    traceReification: options.Contains("--trace-reification"),
    dumpDefinitions: options.Contains("--dump-definitions")
);

if (toolchain.Run(files[0]) is { } failure)
{
    Console.Error.WriteLine($"error: {failure}");

    return 1;
}

return 0;
