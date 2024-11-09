namespace ZSharp.VM
{
    public enum OpCode
    {
        Nop = 0, // nop

        // stack
        Push, // push [value: ZSObject]
        Pop, // pop
        Dup, // dup

        GetArgument, // get-argument [index: i32]
        SetArgument, // set-argument [index: i32]
        GetLocal, // get-local [index: i32]
        SetLocal, // set-local [index: i32]

        // flow control
        Jump, // jump [offset: i32]
        JumpIf, // jump-if [offset: i32]

        Call, // call [argc: i32]
        CallInternal, // call-internal [argc: i32]

        Return, // return
        ReturnVoid, // return-void

        // array
        NewArray, // new-array [length: i32]
        GetMember, // get-member [index: i32]
        SetMember, // set-member [index: i32]

        // oop
        GetField, // get-field [index: i32]
        SetField, // set-field [index: i32]
        CallVirtual, // call-virtual [vinfo: VInfo { type: IR, index: i32, argc: i32 }]

        CreateInstance, // create-instance [constructor: IR]

        LoadObjectFromMetadata, // load-from-metadata [ir: IR]
    }
}
