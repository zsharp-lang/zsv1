using System.Diagnostics;
using System.Text;

// Runs every sample in docs/ as a user would, with the zs CLI, so the documentation says
// what the toolchain does. In a page, a fenced block whose info string is `zs` is a
// whole script, and an `output` block right after it, with only blank lines between, is
// what it prints. A block of `zs fragment` shows syntax and isn't run. A sample runs in a
// folder of its own, which is its working directory.

var root = AppContext.BaseDirectory;
while (!Directory.Exists(Path.Combine(root, "std")) || !Directory.Exists(Path.Combine(root, "docs")))
    root = Path.GetDirectoryName(root) ?? throw new InvalidOperationException("No toolchain directory with docs/ is above this.");

// The CLI built as this is: the configuration is the folder above the framework's.
var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
var cli = Path.Combine(root, "src", "CLI", "ZSharp.CLI", "bin", configuration, "net10.0", "zs.dll");

var failed = 0;
var count = 0;

foreach (var page in Directory.GetFiles(Path.Combine(root, "docs"), "*.md", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
{
    var blocks = Blocks(File.ReadAllLines(page));
    var name = Path.GetRelativePath(root, page).Replace('\\', '/');

    for (var index = 0; index < blocks.Count; index++)
    {
        var block = blocks[index];
        if (block.Info != "zs")
            continue;

        string? expected = null;
        if (index + 1 < blocks.Count && blocks[index + 1].Info == "output" && blocks[index + 1].AfterBlank == block.End)
            expected = blocks[index + 1].Text;

        count++;
        var (ok, why) = Run(block.Text, expected);
        Console.WriteLine($"{(ok ? "pass" : "FAIL")}  {name}:{block.Line}");
        if (!ok)
        {
            failed++;
            Console.WriteLine(Indent(why));
        }
    }
}

if (count == 0)
{
    Console.WriteLine("FAIL  docs/ has no samples");
    failed++;
}

return failed == 0 ? 0 : 1;

// The fenced blocks of a page: each one's info string, its text, the line it starts on,
// where it ends, and where it would start if the blank lines before it weren't there.
static List<Block> Blocks(string[] lines)
{
    var blocks = new List<Block>();
    var index = 0;

    while (index < lines.Length)
    {
        if (!lines[index].StartsWith("```"))
        {
            index++;
            continue;
        }

        var info = lines[index][3..].Trim();
        var start = index;
        var text = new StringBuilder();
        index++;
        while (index < lines.Length && !lines[index].StartsWith("```"))
            text.Append(lines[index++]).Append('\n');
        index++;

        // Where a block would start if only blank lines came between it and the one before.
        var afterBlank = start;
        while (afterBlank > 0 && lines[afterBlank - 1].Trim().Length == 0)
            afterBlank--;

        blocks.Add(new Block(info, text.ToString(), start + 1, index, afterBlank));
    }

    return blocks;
}

(bool, string) Run(string source, string? expected)
{
    var folder = Directory.CreateTempSubdirectory("zsharp-docs-").FullName;
    try
    {
        var script = Path.Combine(folder, "sample.zs");
        File.WriteAllText(script, source);

        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = folder, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add(cli);
        start.ArgumentList.Add(script);

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(TimeSpan.FromMinutes(2)))
        {
            process.Kill(true);
            return (false, "it didn't finish in two minutes");
        }

        var printed = Normalize(output.Result);
        if (process.ExitCode != 0)
            return (false, $"it exited with {process.ExitCode}:\n{printed}\n{Normalize(error.Result)}");

        if (expected is not null && printed != Normalize(expected))
            return (false, $"it printed:\n{printed}\nbut the page says:\n{Normalize(expected)}");

        return (true, "");
    }
    finally
    {
        Directory.Delete(folder, true);
    }
}

static string Normalize(string text)
    => string.Join("\n", text.Replace("\r\n", "\n").Split('\n').Select(line => line.TrimEnd())).Trim();

static string Indent(string text)
    => string.Join("\n", text.Split('\n').Select(line => "      " + line));

record Block(string Info, string Text, int Line, int End, int AfterBlank);
