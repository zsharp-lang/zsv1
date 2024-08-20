﻿namespace ZSharp.CGObjects
{
    public sealed class Module(string? name)
        : CGObject
    {
        public string? Name { get; set; } = name;

        public CGCode Content { get; } = [];
    }
}
