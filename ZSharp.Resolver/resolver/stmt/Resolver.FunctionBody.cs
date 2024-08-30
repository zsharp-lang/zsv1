namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RReturn Resolve(Return @return)
            => new(@return.Value is null ? null : Resolve(@return.Value));
    }
}
