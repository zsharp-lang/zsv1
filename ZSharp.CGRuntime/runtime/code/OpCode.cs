namespace ZSharp.CGRuntime.LLVM
{
    internal enum OpCode
    {
        Get,
        Set,
        Del,

        Object,

        Ret,

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
    }
}
