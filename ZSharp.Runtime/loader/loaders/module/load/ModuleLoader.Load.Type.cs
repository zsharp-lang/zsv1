namespace ZSharp.Runtime.Loaders
{
    partial class ModuleLoader
    {
        public void LoadType(IR.OOPType type)
        {
            switch (type)
            {
                case IR.Class @class: LoadClass(@class); break;
                case IR.Interface @interface: LoadInterface(@interface); break;
                case IR.EnumClass @enum: LoadEnumClass(@enum); break;
                case IR.ValueType valueType: LoadValueType(valueType); break;
                default: throw new NotSupportedException();
            }
        }
    }
}
