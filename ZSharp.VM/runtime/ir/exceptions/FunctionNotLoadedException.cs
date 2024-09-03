namespace ZSharp.VM
{
    public sealed class FunctionNotLoadedException(IR.Function function) 
        : IRObjectNotLoadedException<IR.Function>(function)
    {
        
    }
}
