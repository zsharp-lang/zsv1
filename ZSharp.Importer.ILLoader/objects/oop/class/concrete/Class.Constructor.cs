namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Class
    {
        private CompilerObject? constructor;

        public CompilerObject? Constructor
        {
            get
            {
                if (constructor is null)
                {
                    var constructors = IL.GetConstructors();

                    if (constructors.Length > 1)
                        throw new NotImplementedException("Overloaded constructors are not supported.");

                    if (constructors.Length == 0)
                        return null;

                    constructor = new Constructor(constructors[0], Loader);
                    (constructor as IOnAddTo<Class>)?.OnAddTo(this);
                }

                return constructor;
            }
        }
    }
}
