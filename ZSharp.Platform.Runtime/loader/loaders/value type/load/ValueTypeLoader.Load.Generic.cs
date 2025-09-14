namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ValueTypeLoader
    {
        private void LoadGenericParameters()
        {
            //if (!IRType.HasGenericParameters) return;

            //var ils = ILType.DefineGenericParameters(
            //    [
            //        .. IRType.GenericParameters
            //        .Select(p => p.Name)
            //    ]
            //);

            //foreach (var (ir, il) in IRType.GenericParameters.Zip(ils))
            //    Loader.Runtime.AddType(ir, il);
        }
    }
}
