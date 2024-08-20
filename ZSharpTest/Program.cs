//using CommonZ.Utils;
//using ZSharp.CTRuntime;
//using ZSharp.IR.Standard;
//using ZSharp.RAST;
//using ZSharp.VM;

//using IR = ZSharp.IR;


///*
// * Interpreter
// */

//var runtime = IR.RuntimeModule.Standard;

//Interpreter interpreter = new(runtime);

//{
//    interpreter.SetIR(runtime.TypeSystem.Any, TypeSystem.Any);
//    interpreter.SetIR(runtime.TypeSystem.String, TypeSystem.String);
//    interpreter.SetIR(runtime.TypeSystem.Void, TypeSystem.Void);
//}

///*
// * Toolchain
// */

//Desugarer desugarer = new();
////Instantiator instantiator = new();
//ZSCompiler compiler = new(interpreter);


//#region Internal Functions

//var importSystem = new StringImporter();
//ModuleImporter moduleImporter;

//importSystem.AddImporter("std", moduleImporter = new());


//Mapping<string, IR.InternalFunction> internalFunctions = [];

//if (true) {
//    IR.InternalFunction @internal;
//    ZSInternalFunction impl = new(
//        args =>
//        {
//            if (args.Length != 1)
//                throw new("import: expected 1 argument");

//            var sourceObject = args[0];

//            if (sourceObject is not ZSString sourceString)
//                throw new("import: expected string");

//            var source = sourceString.Value;

//            return importSystem.Import(source);
//        }
//    );

//    @internal = new(runtime.TypeSystem.Void)
//    {
//        Name = "import",
//    };
//    @internal.Signature.Args.Parameters.Add(new("source", runtime.TypeSystem.Void));

//    interpreter.SetIR(@internal, impl);
//    internalFunctions.Add("import", @internal);
//}

//if (true)
//{
//    IR.InternalFunction @internal;
//    ZSInternalFunction impl = new(
//        args =>
//        {
//            if (args.Length != 2)
//                throw new("getattr: expected 2 arguments");

//            var @object = args[0];
//            var name = args[1];

//            if (name is not ZSString nameString)
//                throw new("getattr: name must be a string");

//            if (@object is not ZSModule module)
//                throw new("getattr: object must be a valid module");

//            var binding = compiler.Context.Bindings.Cache(module.Module);
            
//            if (binding is not ICTGetMember<string> getMember)
//                throw new("getattr: object does not support get member");

//            return new ZSHandle<Code>(getMember.Member(compiler, nameString.Value));

//            //IR.IRObject result;

//            //result = 
//            //    (module.Module.Globals.FirstOrDefault(g => g.Name == nameString.Value)
//            //    as IR.IRObject
//            //    )
//            //    ??
//            //    (module.Module.Functions.FirstOrDefault(f => f.Name == nameString.Value)
//            //    as IR.IRObject
//            //    ) ??
//            //    throw new($"Could not find attribute {nameString.Value} in {module}");

//            //Code code = new(1, [
//            //    new IR.VM.GetObject(result)
//            //    ], TypeSystem.Any);

//            //return new ZSHandle<Code>(code);
//        }
//    );

//    @internal = new(runtime.TypeSystem.Any)
//    {
//        Name = "getAttribute",
//    };
//    @internal.Signature.Args.Parameters.Add(new("object", runtime.TypeSystem.Any));
//    @internal.Signature.Args.Parameters.Add(new("name", runtime.TypeSystem.String));
//    interpreter.SetIR(@internal, impl);
//    internalFunctions.Add("getAttribute", @internal);
//}

//if (true)
//{
//    IR.InternalFunction @internal;

//    @internal = new(runtime.TypeSystem.Void)
//    {
//        Name = "print",
//    };
//    @internal.Signature.Args.Parameters.Add(new("message", runtime.TypeSystem.String));

//    ZSInternalFunction impl = new(
//        args =>
//        {
//            if (args.Length != 1)
//                throw new("print: expected 1 argument");

//            var messageObject = args[0];

//            if (messageObject is not ZSString messageString)
//                throw new("print: expected string");

//            var message = messageString.Value;

//            Console.WriteLine(message);

//            return null;
//        },
//        new RTFunctionType(LoadSignature(@internal.Signature))
//    );

//    interpreter.SetIR(@internal, impl);
//    internalFunctions.Add("print", @internal);
//}


//#endregion


//#region Standard Library


//var ioModule = new IR.Module("io");

//if (true) {
//    IR.Function printFunction;

//    printFunction = new(runtime.TypeSystem.Void)
//    {
//        Name = "print",
//    };

//    var sig = printFunction.Signature;
//    sig.Args.Parameters.Add(new("message", runtime.TypeSystem.String));

//    printFunction.Body.Instructions.AddRange([
//        new IR.VM.GetArgument(sig.Args.Parameters[0]),
//        new IR.VM.CallInternal(internalFunctions["print"]),
//        new IR.VM.Return(),
//    ]);

//    printFunction.Body.StackSize = 2;

//    ioModule.Functions.Add(printFunction);
//}

//interpreter.SetIR(ioModule, moduleImporter.AddModule(ioModule));
//compiler.Load(ioModule);


//#endregion


//#region Global Scope


//if (true) {
//    IR.Function importFunction;
//    IR.Parameter source;

//    importFunction = new(runtime.TypeSystem.Any)
//    {
//        Name = "import",
//    };

//    var sig = importFunction.Signature;
//    sig.Args.Parameters.Add(source = new("source", runtime.TypeSystem.String));

//    importFunction.Body.Instructions.AddRange([
//        new IR.VM.GetArgument(source),
//        new IR.VM.CallInternal(internalFunctions["import"]),
//        new IR.VM.Return(),
//    ]);

//    importFunction.Body.StackSize = 2;
//    compiler.DefineGlobal(
//        desugarer.Context.ImportFunction, 
//        new RTFunction(importFunction, LoadSignature(sig))
//        );
//}

//if (true)
//{
//    IR.Function getattrFunction;
//    IR.Parameter @object, name;

//    getattrFunction = new(runtime.TypeSystem.Any)
//    {
//        Name = "_._"
//    };

//    var sig = getattrFunction.Signature;
//    sig.Args.Parameters.Add(@object = new("object", runtime.TypeSystem.Any));
//    sig.Args.Parameters.Add(name = new("name", runtime.TypeSystem.String));

//    getattrFunction.Body.Instructions.AddRange([
//        new IR.VM.GetArgument(@object),
//        new IR.VM.GetArgument(name),
//        new IR.VM.CallInternal(internalFunctions["getAttribute"]),
//        new IR.VM.Return()
//    ]);

//    getattrFunction.Body.StackSize = 3;
//    compiler.DefineGlobal(
//        desugarer.Context.GetBinaryOperator("."), 
//        new CGFunction(getattrFunction, LoadSignature(sig))
//        );
//}


//RId globalVoid = new("void");
//compiler.DefineGlobal(globalVoid, new Class(runtime.TypeSystem.Void));
//RId globalString = new("string");
//compiler.DefineGlobal(globalString, new Class(runtime.TypeSystem.String));


//#endregion


//#region RAST

///*
// * The code below represents the RAST of the following Z# code:
// * 
// * let stdIO = "std:io";
// * import { print } from stdIO;
// * 
// * fun id(x: string): string {
// *   return x;
// * }
// * 
// * let message = id("Hello, World!");
// * print(message);
// * 
// * fun foo(x: string): void {
// *   print(x);
// * }
// * 
// * foo("Hello, foo!");
// */


//RId stdIO;
//RId print;
//RId id;
//RId id_x;
//RId message;
//RId foo_x;
//RId foo;
//var rastNodes = new RStatement[]
//{
//    // let stdIO = "std:io";
//    new RLetStatement(
//        new RNamePattern(stdIO = new("stdIO")),
//        null,
//        RLiteral.String("std:io")
//    ),

//    // import { print } from stdIO;
//    new RImport(stdIO)
//    {
//        Targets = [
//            new("print")
//        ]
//    },

//    new RExpressionStatement(
//        new RFunction(
//            id = new("id"), 
//            new()
//            {
//                Args = [
//                    new(id_x = new("x"), globalString, null)
//                ]
//            })
//        {
//            ReturnType = globalString,
//            Body = new RReturn(id_x)
//        }
//    ),

//    // let message = "Hello, World!";
//    new RLetStatement(
//        new RNamePattern(message = new("message")),
//        null, //globals["string"],
//        new RCall(
//            id, 
//            [ 
//                new(RLiteral.String("Hello, World!"))
//            ]
//        )
//    ),

//    // print(message);
//    new RExpressionStatement(
//        new RCall(print,
//        [
//            new(message)
//        ])
//    ),

//    // fun foo(x: string): void { print(x); }
//    new RExpressionStatement(
//        new RFunction(
//            foo = new("foo"),
//            new() {
//                Args =
//                [
//                    new(foo_x = new("x"), globalString, null)
//                ]
//            })
//        {
//            ReturnType = globalVoid,
//            Body = new RReturn(
//                new RCall(print,
//                [
//                    new(foo_x)
//                ])
//            )
//        }
//    ),

//    // foo("Hello, foo!");
//    new RExpressionStatement(
//        new RCall(foo,
//        [
//            new(RLiteral.String("Hello, foo!"))
//        ])
//    )
//};

//RModule moduleNode = new(string.Empty) { Content = new(new(rastNodes)) };

//#endregion


//#region Main

//// Simplify the RAST
//moduleNode = desugarer.Desugar(moduleNode);

//// Declare module contents
////var scopes = instantiator.Instantiate(moduleNode);

//// Document module
//IR.Module module;

//// Definitions
//using (compiler.UseContext()) // if you want to pre-initialize, pass compiler.Context to the instantiator and use NewScope
//    module = compiler.Compile(moduleNode);


//var moduleObject = interpreter.LoadIR(module) as ZSModule;

//Console.WriteLine("Program finished!");

//#endregion


//#region Utilities

//Signature LoadSignature(IR.Signature signature)
//{
//    var result = new Signature(signature);

//    foreach (var parameter in signature.Args.Parameters)
//        result.Args.Add(new(parameter, interpreter.LoadType(parameter.Type)));

//    return result;
//}

//#endregion
