using ZSharp.Compiler.ILLoader;
using Objects = ZSharp.Compiler.ILLoader.Objects;

var rtm = ZSharp.IR.RuntimeModule.Standard;
var coc = new ZSharp.Compiler.Compiler(rtm);
var rt = new ZSharp.Runtime.Runtime(new()
{
    Array = rtm.TypeSystem.Array,
    Boolean = rtm.TypeSystem.Boolean,
    Char = null!,
    Float16 = null!,
    Float32 = rtm.TypeSystem.Float32,
    Float64 = null!,
    Float128 = null!,
    Object = rtm.TypeSystem.Object,
    Pointer = rtm.TypeSystem.Pointer,
    Reference = rtm.TypeSystem.Reference,
    SInt8 = null!,
    SInt16 = null!,
    SInt32 = rtm.TypeSystem.Int32,
    SInt64 = null!,
    SIntNative = null!,
    String = rtm.TypeSystem.String,
    UInt8 = null!,
    UInt16 = null!,
    UInt32 = null!,
    UInt64 = null!,
    UIntNative = null!,
    Void = rtm.TypeSystem.Void
});

var ilLoader = new ILLoader(coc.IR, rt);

coc.TS.SInt32 = ilLoader.TypeSystem.SInt32;

var code = rt.Expose("Hello, Exposed String!");
var exposed = rt.Evaluate(code, rtm.TypeSystem.Object);
System.Console.WriteLine(exposed);

var typedCode = ilLoader.ExposeAsIR(12);
var typedExposed = rt.Evaluate(typedCode, rtm.TypeSystem.Int32);
System.Console.WriteLine(typedExposed);


System.Console.WriteLine(string.Empty);
