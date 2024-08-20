using ZSharp.Compiler;
using ZSharp.RAST;

var compiler = new Compiler();

#region RAST

/*
 * The code below represents the RAST of the following Z# code:
 * 
 * let stdIO = "std:io";
 * import { print } from stdIO;
 * 
 * fun id(x: string): string {
 *   return x;
 * }
 * 
 * let message = id("Hello, World!");
 * print(message);
 * 
 * fun foo(x: string): void {
 *   print(x);
 * }
 * 
 * foo("Hello, foo!");
 */


//RId stdIO;
RId print;
//RId id;
//RId id_x;
//RId message;
//RId foo_x;
//RId foo;
var rastNodes = new RStatement[]
{
    //// let stdIO = "std:io";
    //new RLetStatement(
    //    new RNamePattern(stdIO = new("stdIO")),
    //    null,
    //    RLiteral.String("std:io")
    //),

    // import { print } from stdIO;
    new RImport(RLiteral.String("std:io"))
    {
        Targets = [
            new(print = new("print"))
        ]
    },

    //new RExpressionStatement(
    //    new RFunction(
    //        id = new("id"),
    //        new()
    //        {
    //            Args = [
    //                new(id_x = new("x"), globalString, null)
    //            ]
    //        })
    //    {
    //        ReturnType = globalString,
    //        Body = new RReturn(id_x)
    //    }
    //),

    //// let message = "Hello, World!";
    //new RLetStatement(
    //    new RNamePattern(message = new("message")),
    //    null, //globals["string"],
    //    new RCall(
    //        id,
    //        [
    //            new(RLiteral.String("Hello, World!"))
    //        ]
    //    )
    //),

    //// print(message);
    //new RExpressionStatement(
    //    new RCall(print,
    //    [
    //        new(message)
    //    ])
    //),

    // print("Hello");
    new RExpressionStatement(
        new RCall(print,
        [
            new(RLiteral.String("Hello"))
        ])
    ),

    //// fun foo(x: string): void { print(x); }
    //new RExpressionStatement(
    //    new RFunction(
    //        foo = new("foo"),
    //        new() {
    //            Args =
    //            [
    //                new(foo_x = new("x"), globalString, null)
    //            ]
    //        })
    //    {
    //        ReturnType = globalVoid,
    //        Body = new RReturn(
    //            new RCall(print,
    //            [
    //                new(foo_x)
    //            ])
    //        )
    //    }
    //),

    //// foo("Hello, foo!");
    //new RExpressionStatement(
    //    new RCall(foo,
    //    [
    //        new(RLiteral.String("Hello, foo!"))
    //    ])
    //)
};

#endregion


var cgCode = compiler.CompileCG(rastNodes);
var module = compiler.CompileIR(cgCode);

Console.ReadLine();
