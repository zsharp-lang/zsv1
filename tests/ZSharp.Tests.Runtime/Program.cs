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

// A function that belongs to no module is a method of its own, and calls itself.
var subtract = new L.FunctionHandle(Sig(i32, i32, i32));
runtime.Map(subtract, il => il.Emit(OpCodes.Sub));

var triangleSig = Sig(i32, i32);
var triangle = new L.Function(triangleSig) { Name = "triangle", Body = new L.Code() };
triangle.Body.Block.Statements.Add(new L.If(
    new L.Call(less, new L.GetLocal(triangleSig.Parameters[0]), I(1)),
    new L.Return(I(0)),
    new L.Return(new L.Call(add,
        new L.GetLocal(triangleSig.Parameters[0]),
        new L.Call(triangle, new L.Call(subtract, new L.GetLocal(triangleSig.Parameters[0]), I(1)))))
));
Report("a standalone function calls itself", (int)runtime.LoadMethod(triangle).Invoke(null, [4])! == 10);

// A module function calls it through its delegate, and it calls a module function.
var callers = new L.Module("Callers");
var callsStandalone = Fn(callers, "callsStandalone", Sig(i32), code =>
    code.Block.Statements.Add(new L.Return(new L.Call(add, new L.Call(triangle, I(3)), I(1)))));
runtime.Load(callers);
Report("a module function calls a standalone function", (int)Run(callsStandalone)! == 7);

var callsModule = new L.Function(Sig(i32)) { Name = "callsModule", Body = new L.Code() };
callsModule.Body.Block.Statements.Add(new L.Return(new L.Call(sum, I(4))));
Report("a standalone function calls a module function", (int)runtime.LoadMethod(callsModule).Invoke(null, [])! == 6);

// Globals with initializers: each has its value before it is first used, and an
// initializer can call a function and use a global declared before it.
var initializing = new L.Module("Initializing");
var base_ = new L.Global(i32) { Name = "base", Initializer = new L.Call(add, I(40), I(1)) };
initializing.Globals.Add(base_);
var next = new L.Global(i32) { Name = "next", Initializer = new L.Call(add, new L.GetGlobal(base_), I(1)) };
initializing.Globals.Add(next);
var readsNext = Fn(initializing, "readsNext", Sig(i32), code =>
    code.Block.Statements.Add(new L.Return(new L.GetGlobal(next))));
runtime.Load(initializing);
Report("a global has its initializer's value before it is first used", (int)Run(readsNext)! == 42);

// Polymorphic structures, as a platform with polymorphism marks them: an interface with
// an abstract method, and a class that implements it, called through the interface.
var multiply = new L.FunctionHandle(Sig(i32, i32, i32));
runtime.Map(multiply, il => il.Emit(OpCodes.Mul));

var shapes = new L.Module("Shapes");
var shape = new L.Structure { Name = "Shape" };
shape.Metadata.Add(new Polymorphic { IsInterface = true });
shapes.Structures.Add(shape);

var area = new L.Function(Sig(i32, shape)) { Name = "area" };
area.Metadata.Add(new MethodOf(shape));
shapes.Functions.Add(area);

var square = new L.Structure { Name = "Square" };
var side = new L.Field(i32) { Name = "side" };
square.Fields.Add(side);
square.Metadata.Add(new Polymorphic { Interfaces = { shape } });
shapes.Structures.Add(square);

var squareAreaSig = Sig(i32, square);
var squareArea = Fn(shapes, "area", squareAreaSig, code =>
{
    var self = new L.GetLocal(squareAreaSig.Parameters[0]);
    code.Block.Statements.Add(new L.Return(new L.Call(multiply, new L.GetField(self, side), new L.GetField(self, side))));
});
squareArea.Metadata.Add(new MethodOf(square));

var makeSig = Sig(shape, i32);
var make = Fn(shapes, "make", makeSig, code =>
    code.Block.Statements.Add(new L.Return(new L.New(square, new L.GetLocal(makeSig.Parameters[0])))));

var useShape = Fn(shapes, "use", Sig(i32), code =>
    code.Block.Statements.Add(new L.Return(new L.Call(area, new L.Call(make, I(3))))));

runtime.Load(shapes);
Report("a method called through an interface runs what the class has", (int)Run(useShape)! == 9);
Report("a polymorphic structure is a CLR class that implements the interface",
    Run(make, 4) is { } made && !made.GetType().IsValueType && made.GetType().GetInterfaces().Any(@interface => @interface.Name == "Shape"));

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
