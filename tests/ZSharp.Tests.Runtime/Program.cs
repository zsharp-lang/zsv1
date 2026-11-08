using System.Reflection.Emit;
using L = ZSharp.LIR;
using ZSharp.Platform.Runtime;

void Report(string name, bool ok)
{
    if (!ok)
        Environment.ExitCode = 1;

    Console.WriteLine($"{(ok ? "pass" : "FAIL")}  {name}");
}

var runtime = new Runtime();

var str = new L.TypeHandle();
var i32 = new L.TypeHandle();
var boolean = new L.TypeHandle();
runtime.Map(str, typeof(string));
runtime.Map(i32, typeof(int));
runtime.Map(boolean, typeof(bool));

L.Signature Sig(L.Type? ret, params L.Type[] parameters)
{
    var signature = new L.Signature { ReturnType = ret };
    foreach (var parameter in parameters)
        signature.Parameters.Add(new L.Parameter(parameter));
    return signature;
}

L.Function Fn(L.Module module, string name, L.Signature signature, Action<L.Code> body)
{
    var code = new L.Code();
    body(code);
    var function = new L.Function(signature) { Name = name, Body = code };
    module.Functions.Add(function);
    return function;
}

L.Expression I(int value) => new L.Constant(i32, value);

var printed = new StringWriter();

var print = new L.FunctionHandle(Sig(null, str));
runtime.Map(print, typeof(Sink).GetMethod(nameof(Sink.Print))!);
Sink.Target = printed;

var add = new L.FunctionHandle(Sig(i32, i32, i32));
runtime.Map(add, il => il.Emit(OpCodes.Add));
var less = new L.FunctionHandle(Sig(boolean, i32, i32));
runtime.Map(less, il => il.Emit(OpCodes.Clt));

var main = new L.Module("Main");

// print("Hello")
var hello = Fn(main, "hello", Sig(null), code =>
    code.Block.Statements.Add(new L.Evaluate(new L.Call(print, new L.Constant(str, "Hello")))));

// Loop, locals, inline handles: sum 0..n-1.
var sumSig = Sig(i32, i32);
var sum = Fn(main, "sum", sumSig, code =>
{
    var i = new L.Local(i32) { Name = "i" };
    var total = new L.Local(i32) { Name = "total" };
    code.Block.Locals.Add(i);
    code.Block.Locals.Add(total);
    var loop = new L.Loop(new L.Block());
    loop.Body = new L.Block(
        new L.If(new L.Call(less, new L.GetLocal(i), new L.GetLocal(sumSig.Parameters[0])), new L.Block(), new L.Break(loop)),
        new L.SetLocal(total, new L.Call(add, new L.GetLocal(total), new L.GetLocal(i))),
        new L.SetLocal(i, new L.Call(add, new L.GetLocal(i), I(1))),
        new L.Continue(loop)
    );
    code.Block.Statements.Add(loop);
    code.Block.Statements.Add(new L.Return(new L.GetLocal(total)));
});

// A structure that refers to itself through a reference.
var node = new L.Structure { Name = "Node" };
var nodeRef = new L.Reference(node);
var value = new L.Field(i32) { Name = "value" };
var parent = new L.Field(nodeRef) { Name = "parent" };
node.Fields.Add(value);
node.Fields.Add(parent);
main.Structures.Add(node);

var linked = Fn(main, "linked", Sig(i32), code =>
{
    var a = new L.Local(nodeRef) { Name = "a" };
    var b = new L.Local(nodeRef) { Name = "b" };
    code.Block.Locals.Add(a);
    code.Block.Locals.Add(b);
    code.Block.Statements.Add(new L.SetLocal(a, new L.Allocate(new L.New(node, I(1), new L.Constant(nodeRef, null)))));
    code.Block.Statements.Add(new L.SetLocal(b, new L.Allocate(new L.New(node, I(2), new L.GetLocal(a)))));
    code.Block.Statements.Add(new L.SetField(new L.GetLocal(a), value, I(40)));
    code.Block.Statements.Add(new L.Return(new L.Call(add,
        new L.GetField(new L.GetField(new L.GetLocal(b), parent), value),
        new L.GetField(new L.GetLocal(b), value))));
});

// Value semantics: copying an inline structure does not alias it.
var copies = Fn(main, "copies", Sig(i32), code =>
{
    var a = new L.Local(node) { Name = "a" };
    var b = new L.Local(node) { Name = "b" };
    code.Block.Locals.Add(a);
    code.Block.Locals.Add(b);
    code.Block.Statements.Add(new L.SetLocal(a, new L.New(node, I(5), new L.Constant(nodeRef, null))));
    code.Block.Statements.Add(new L.SetLocal(b, new L.GetLocal(a)));
    code.Block.Statements.Add(new L.SetField(new L.GetLocal(b), value, I(9)));
    code.Block.Statements.Add(new L.Return(new L.GetField(new L.GetLocal(a), value)));
});

// References do alias, through Load and Store.
var shared = Fn(main, "shared", Sig(i32), code =>
{
    var intRef = new L.Reference(i32);
    var a = new L.Local(intRef) { Name = "a" };
    var b = new L.Local(intRef) { Name = "b" };
    code.Block.Locals.Add(a);
    code.Block.Locals.Add(b);
    code.Block.Statements.Add(new L.SetLocal(a, new L.Allocate(I(1))));
    code.Block.Statements.Add(new L.SetLocal(b, new L.GetLocal(a)));
    code.Block.Statements.Add(new L.Store(new L.GetLocal(b), I(7)));
    code.Block.Statements.Add(new L.Return(new L.Load(new L.GetLocal(a))));
});

// A global holding an inline structure, written through its field.
var root = new L.Global(node) { Name = "root" };
main.Globals.Add(root);
var globals = Fn(main, "globals", Sig(i32), code =>
{
    code.Block.Statements.Add(new L.SetGlobal(root, new L.New(node, I(3), new L.Constant(nodeRef, null))));
    code.Block.Statements.Add(new L.SetField(new L.GetGlobal(root), value, I(8)));
    code.Block.Statements.Add(new L.Return(new L.GetField(new L.GetGlobal(root), value)));
});

// A function value called indirectly.
var callback = new L.FunctionType(Sig(i32, i32));
var indirect = Fn(main, "indirect", Sig(i32), code =>
{
    var fn = new L.Local(callback) { Name = "fn" };
    code.Block.Locals.Add(fn);
    code.Block.Statements.Add(new L.SetLocal(fn, new L.GetFunction(sum)));
    code.Block.Statements.Add(new L.Return(new L.CallIndirect(new L.GetLocal(fn), callback.Signature, I(4))));
});

// Statements in expression position.
var sequence = Fn(main, "sequence", Sig(i32), code =>
{
    var t = new L.Local(i32) { Name = "t" };
    code.Block.Locals.Add(t);
    code.Block.Statements.Add(new L.Return(new L.Call(add,
        I(1),
        new L.Sequence(new L.Block(new L.SetLocal(t, I(20))), new L.Call(add, new L.GetLocal(t), new L.GetLocal(t)))
    )));
});

// Two modules that call each other.
var left = new L.Module("Left");
var right = new L.Module("Right");
var evenSig = Sig(boolean, i32);
var oddSig = Sig(boolean, i32);
var odd = new L.Function(oddSig) { Name = "odd" };
right.Functions.Add(odd);
var even = Fn(left, "even", evenSig, code =>
{
    code.Block.Statements.Add(new L.If(
        new L.Call(less, new L.GetLocal(evenSig.Parameters[0]), I(1)),
        new L.Return(new L.Constant(boolean, true))
    ));
    code.Block.Statements.Add(new L.Return(new L.Call(odd, new L.Call(add, new L.GetLocal(evenSig.Parameters[0]), I(-1)))));
});
odd.Body = new L.Code();
odd.Body.Block.Statements.Add(new L.If(
    new L.Call(less, new L.GetLocal(oddSig.Parameters[0]), I(1)),
    new L.Return(new L.Constant(boolean, false))
));
odd.Body.Block.Statements.Add(new L.Return(new L.Call(even, new L.Call(add, new L.GetLocal(oddSig.Parameters[0]), I(-1)))));

runtime.Load(main);
runtime.Load(left);

object? Run(L.Function function, params object[] arguments)
    => runtime.GetMethod(function).Invoke(null, arguments);

Run(hello);
Report("print through a mapped handle", printed.ToString() == "Hello");
Report("loop, locals and inline handles", (int)Run(sum, 5)! == 10);
Report("self-referencing structure through references", (int)Run(linked)! == 42);
Report("inline structures copy", (int)Run(copies)! == 5);
Report("references alias", (int)Run(shared)! == 7);
Report("global written through a field", (int)Run(globals)! == 8);
Report("indirect call", (int)Run(indirect)! == 6);
Report("sequence in expression position", (int)Run(sequence)! == 41);
Report("modules calling each other", (bool)Run(even, 6)! && !(bool)Run(even, 7)!);
Report("other module loaded by reachability", (bool)Run(odd, 7)!);

// A later load reaching what is already complete.
var later = new L.Module("Later");
var reuse = Fn(later, "reuse", Sig(i32), code =>
{
    var a = new L.Local(nodeRef) { Name = "a" };
    code.Block.Locals.Add(a);
    code.Block.Statements.Add(new L.SetLocal(a, new L.Allocate(new L.New(node, new L.Call(sum, I(4)), new L.Constant(nodeRef, null)))));
    code.Block.Statements.Add(new L.Return(new L.GetField(new L.GetLocal(a), value)));
});
runtime.Load(later);
Report("later module uses loaded function and structure", (int)Run(reuse)! == 6);

var unmapped = new L.Module("Unmapped");
var missing = new L.FunctionHandle(Sig(null));
Fn(unmapped, "bad", Sig(null), code => code.Block.Statements.Add(new L.Evaluate(new L.Call(missing))));
var failed = false;
try { new Runtime().Load(unmapped); } catch (InvalidOperationException) { failed = true; }
Report("unmapped handle is an error", failed);

public static class Sink
{
    public static TextWriter Target = TextWriter.Null;
    public static void Print(string message) => Target.Write(message);
}
