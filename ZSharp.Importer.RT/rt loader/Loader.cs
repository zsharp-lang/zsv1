namespace ZSharp.Importer.RT
{
    public sealed partial class Loader
    {
        public Loader(ILLoader.ILLoader ilLoader)
        {
            ILLoader = ilLoader;
            LoadObject = DefaultLoader;
        }
    }
}
