﻿using System.Diagnostics.Tracing;
using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ClassSpecCompiler
    {
        //private void Declare(Class @class, ROOPDefinition node)
        //{
        //    if (node.Bases is not null)
        //        foreach (var baseObject in node.Bases)
        //            @class.Bases.Add(Compiler.CompileNode(baseObject));

        //    var bases = new Queue<IRType>((@class.Bases ?? [])
        //        .Select(Compiler.CompileIRType));

        //    IR.Class? @base = null;
        //    if (bases.Count > 0)
        //    {
        //        if (bases.Peek() is IR.Class) @base = (IR.Class)bases.Dequeue();

        //        foreach (var item in bases)
        //        {
        //            throw new NotImplementedException();

        //            //if (item is not IR.Interface @interface)
        //            //    throw new();

        //            //@class.Interfaces.Add(@interface);
        //        }
        //    }

        //    @class.IR = new(node.Name, @base);

        //    Result.IR!.Types.Add(@class.IR);

        //    // TODO: implement class parameters
        //}

        private void Declare(Field field, RLetDefinition node)
        {
            field.Initializer = Compiler.CompileNode(node.Value);

            if (node.Type is not null)
                field.Type = Compiler.CompileNode(node.Type);

            var type = field.Type is null ? null : Compiler.CompileIRType(field.Type);
            var initializer = Compiler.CompileIRCode(field.Initializer);

            type ??= initializer.RequireValueType();

            field.IR = new IR.Field(field.Name, type)
            {
                Initializer = [.. initializer.Instructions],
                IsReadOnly = true,
            };
        }

        //private void Declare(Global global, RVarDefinition node)
        //{
        //    if (node.Value is not null)
        //        global.Initializer = Compiler.CompileNode(node);

        //    if (node.Type is not null)
        //        global.Type = Compiler.CompileNode(node.Type);

        //    var type = global.Type is null 
        //        ? null 
        //        : Compiler.CompileIRType(global.Type);
        //    var initializer = global.Initializer is null 
        //        ? null 
        //        : Compiler.CompileIRCode(global.Initializer);

        //    type ??= initializer?.RequireValueType();

        //    if (type is null)
        //        throw new();

        //    global.IR = new IR.Global(global.Name, type)
        //    {
        //        Initializer = initializer is null ? null : [.. initializer.Instructions]
        //    };

        //    Result.IR!.Globals.Add(global.IR);
        //}

        private void Declare(Method method, RFunction node)
        {
            //IR.Parameter Declare(Parameter parameter)
            //{
            //    var node = (RParameter)objectBuilder.Nodes.Cache(parameter)!.Node;

            //    parameter.Type =
            //        node.Type is null ? null :
            //        Compiler.CompileNode(node.Type);

            //    parameter.Initializer =
            //        node.Default is null ? null :
            //        Compiler.CompileNode(node.Default);

            //    var initializer = parameter.Initializer is null
            //        ? null
            //        : Compiler.CompileIRCode(parameter.Initializer);
            //    var type = parameter.Type is null
            //        ? null
            //        : Compiler.CompileIRType(parameter.Type);

            //    if (type is null && initializer is null)
            //        throw new();
            //    else type ??= initializer!.RequireValueType();

            //    Context.CurrentScope.Cache(parameter.Name, parameter);

            //    return parameter.IR = new(parameter.Name, type)
            //    {
            //        Initializer = initializer?.Instructions.ToArray()
            //    };
            //}

            //using (Context.For(method))
            //{

            //    if (node.ReturnType is not null)
            //        method.ReturnType = Compiler.CompileNode(node.ReturnType);

            //    IRType ? returnType = null;

            //    if (method.ReturnType is not null)
            //        returnType = Compiler.CompileIRType(method.ReturnType);

            //    if (returnType is null)
            //        throw new NotImplementedException("Implicit return type");

            //    method.IR = new(returnType)
            //    {
            //        Name = method.Name,
            //    };

            //    Result.IR!.Functions.Add(method.IR);

            //    foreach (var arg in method.Signature.Args)
            //        method.IR.Signature.Args.Parameters.Add(Declare(arg));

            //    if (method.Signature.VarArgs is not null)
            //        method.IR.Signature.Args.Var = Declare(method.Signature.VarArgs);

            //    foreach (var arg in method.Signature.KwArgs)
            //        method.IR.Signature.KwArgs.Parameters.Add(Declare(arg));

            //    if (method.Signature.VarKwArgs is not null)
            //        method.IR.Signature.KwArgs.Var = Declare(method.Signature.VarKwArgs);
            //}
        }
    }
}
