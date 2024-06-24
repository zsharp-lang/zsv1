namespace ZSharp.VM
{
    public static class CoreTypes
    {
        public static ZSObject Any { get; } = new Types.Any();

        //public static ZSObject Null { get; } = new Types.Null();

        public static ZSObject Module { get; } = Any;
    }
}
