using CommonZ.Utils;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private readonly Mapping<Type, object> features = [];

        private void InitializeFeatures()
        {
            
        }

        public T Feature<T>()
            where T : class
            => (T)features[typeof(T)];

        public void Feature<T>(T feature)
            where T : Feature
            => features[typeof(T)] = feature;
            
        public void Feature(Feature feature)
            => features[feature.GetType()] = feature;
    }
}
