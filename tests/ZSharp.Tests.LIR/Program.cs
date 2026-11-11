using L = ZSharp.LIR;

var str = new L.TypeHandle();
var sig = new L.Signature();
sig.Parameters.Add(new L.Parameter(str) { Name = "message" });
L.ICallable print = new L.FunctionHandle(sig);
Console.WriteLine($"handle is callable with signature: {print.Signature.Parameters[0].Type == str}");

var a = new L.Module("A");
var b = new L.Module("B");

var node = new L.Structure { Name = "Node" };
node.Fields.Add(new L.Field(new L.Reference(node)) { Name = "parent" });
a.Structures.Add(node);

var g = new L.Global(node) { Name = "root" };
a.Globals.Add(g);

var f = new L.Function(new L.Signature { ReturnType = node }) { Name = "getRoot" };
b.Functions.Add(f);

Console.WriteLine($"owner set: {g.Module == a}");
Console.WriteLine($"cross-module type: {f.Signature.ReturnType == node && f.Module == b}");
Console.WriteLine($"self reference: {((L.Reference)node.Fields[0].Type).Target == node}");

try { b.Globals.Add(g); Console.WriteLine("double ownership NOT rejected"); }
catch (InvalidOperationException) { Console.WriteLine("double ownership rejected"); }

Console.WriteLine($"failed add left list unchanged: {b.Globals.Count == 0 && g.Module == a}");

a.Globals.Remove(g);
Console.WriteLine($"owner cleared: {g.Module is null}");

b.Globals.Add(g);
Console.WriteLine($"re-add works: {g.Module == b}");

a.Structures[0] = a.Structures[0];
Console.WriteLine($"same-item set keeps owner: {node.Module == a}");

var other = new L.Structure();
a.Structures[0] = other;
Console.WriteLine($"replace: {node.Module is null && other.Module == a}");

a.Structures.Clear();
Console.WriteLine($"clear: {other.Module is null}");
