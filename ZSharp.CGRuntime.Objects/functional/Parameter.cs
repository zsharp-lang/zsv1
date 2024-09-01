﻿using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class Parameter(string name) 
        : CGObject
        , ICTReadable
    {
        public IR.Parameter? IR { get; set; }

        public string Name { get; } = name;

        public CGCode? Type { get; init; }

        public CGCode? Initializer { get; init;}

        IType ICTReadable.Type => IR!.Type;

        public Code Read(ICompiler compiler)
            => new([
                new IR.VM.GetArgument(IR!),
            ])
            {
                MaxStackSize = 1,
            };
    }
}
