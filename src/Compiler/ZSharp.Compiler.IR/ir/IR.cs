namespace ZSharp.Compiler
{
    public partial class IR
    {
        public IR Clone() => (IR)MemberwiseClone();
    }
}
