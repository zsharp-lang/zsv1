namespace ZSharp.Runtime.Loaders
{
    partial class EnumClassLoader
    {
        private void LoadValue(IR.EnumValue value)
        {
            var il = ILType.DefineField(
                value.Name, 
                ILType,
                IL.FieldAttributes.Public | IL.FieldAttributes.Static
            );

            // TODO: add caching when adding IR instruction for loading enum value
            //Loader.Runtime.AddField(value, il);
        }
    }
}
