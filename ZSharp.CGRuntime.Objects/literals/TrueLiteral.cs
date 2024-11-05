﻿using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public sealed class TrueLiteral(IType type)
        : CGObject
        , ICTReadable
    {
        public IType Type { get; } = type;

        public Code Read(ICompiler compiler)
            => new([
                new IR.VM.PutTrue()
            ])
            {
                MaxStackSize = 1,
                Types = [Type]
            };
    }
}
