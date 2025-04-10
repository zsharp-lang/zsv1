﻿using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Import : CompilerObject
    {
        public required List<Argument> Arguments { get; set; }

        public required List<ImportedName> ImportedNames { get; set; }

        public string? Alias { get; set; }
    }
}
