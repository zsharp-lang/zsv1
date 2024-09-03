namespace ZSharp.CGRuntime.LLVM
{
    internal enum OpCode
    {
        Get,

        Object,

        End,

        Argument,
        Call,

        Cast,

        GetMemberName,
        SetMemberName,
        GetMemberIndex,
        SetMemberIndex,

        GetIndex,
        SetIndex,
        DelIndex,

        Assign,

        Literal,

        Evaluate,

        Jump,
        JumpIfTrue,
        JumpIfFalse,

        Inject,

        Label,

        Enter,
        Leave,

        Compile,

        Dup,
    }
}
