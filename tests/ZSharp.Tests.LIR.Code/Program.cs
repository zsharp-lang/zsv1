using ZSharp.LIR;

// A toy target: reads LIR and prints Python-like text. If this can be written
// without knowing anything but the nodes, the tree reads the way it should.

string E(Expression e, Dictionary<object, string> n) => e switch
{
    Constant c => c.Value is string s ? $"\"{s}\"" : c.Value?.ToString() ?? "None",
    GetLocal g => ((Definition)g.Variable).Name!,
    GetGlobal g => g.Global.Name!,
    GetField g => $"{E(g.Target, n)}.{g.Field.Name}",
    GetFunction g => n[g.Callable],
    Call c => $"{n[c.Callable]}({string.Join(", ", c.Arguments.Select(a => E(a, n)))})",
    CallIndirect c => $"{E(c.Callee, n)}({string.Join(", ", c.Arguments.Select(a => E(a, n)))})",
    New x => $"{x.Structure.Name}({string.Join(", ", x.Fields.Select(a => E(a, n)))})",
    Allocate a => E(a.Value, n),
    Load l => E(l.Reference, n),
    _ => throw new InvalidOperationException(e.GetType().Name),
};

void S(Statement s, Dictionary<object, string> n, string i)
{
    switch (s)
    {
        case Block b: foreach (var x in b.Statements) S(x, n, i); break;
        case Evaluate e: Console.WriteLine($"{i}{E(e.Expression, n)}"); break;
        case SetLocal x: Console.WriteLine($"{i}{((Definition)x.Variable).Name} = {E(x.Value, n)}"); break;
        case SetField x: Console.WriteLine($"{i}{E(x.Target, n)}.{x.Field.Name} = {E(x.Value, n)}"); break;
        case If x:
            Console.WriteLine($"{i}if {E(x.Condition, n)}:");
            S(x.Then, n, i + "    ");
            if (x.Else is not null) { Console.WriteLine($"{i}else:"); S(x.Else, n, i + "    "); }
            break;
        case Loop x: Console.WriteLine($"{i}while True:"); S(x.Body, n, i + "    "); break;
        case Break: Console.WriteLine($"{i}break"); break;
        case Continue: Console.WriteLine($"{i}continue"); break;
        case Return x: Console.WriteLine($"{i}return {(x.Value is null ? "" : E(x.Value, n))}"); break;
        default: throw new InvalidOperationException(s.GetType().Name);
    }
}

var str = new TypeHandle();
var boolean = new TypeHandle();

var printSig = new Signature();
printSig.Parameters.Add(new Parameter(str) { Name = "message" });
var print = new FunctionHandle(printSig);

var lessSig = new Signature { ReturnType = boolean };
var less = new FunctionHandle(lessSig);

var module = new Module("Main");

var node = new Structure { Name = "Node" };
var parent = new Field(new Reference(node)) { Name = "parent" };
node.Fields.Add(parent);
module.Structures.Add(node);

var helper = new Function(new Signature()) { Name = "helper" };
module.Functions.Add(helper);

var main = new Function(new Signature()) { Name = "main" };
var body = new Code();
var count = new Local(str) { Name = "count" };
var first = new Local(new Reference(node)) { Name = "first" };
body.Block.Locals.Add(count);
body.Block.Locals.Add(first);

var callback = new FunctionType(new Signature());
var fn = new Local(callback) { Name = "fn" };
body.Block.Locals.Add(fn);

body.Block.Statements.Add(new Evaluate(new Call(print, new Constant(str, "Hello"))));
body.Block.Statements.Add(new SetLocal(first, new Allocate(new New(node, new Constant(new Reference(node), null)))));
body.Block.Statements.Add(new SetField(new GetLocal(first), parent, new GetLocal(first)));
body.Block.Statements.Add(new SetLocal(fn, new GetFunction(helper)));
body.Block.Statements.Add(new Evaluate(new CallIndirect(new GetLocal(fn), callback.Signature)));
body.Block.Statements.Add(new Loop(new Block(
    new If(new Call(less, new GetLocal(count)), new Break(), new Continue())
)));
body.Block.Statements.Add(new Return());
main.Body = body;
module.Functions.Add(main);

var names = new Dictionary<object, string> { [print] = "print", [less] = "done", [helper] = "helper" };

Console.WriteLine($"def {main.Name}():");
S(main.Body.Block, names, "    ");
