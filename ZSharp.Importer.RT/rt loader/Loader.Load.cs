namespace ZSharp.Importer.RT
{
    public delegate IResult LoadObject(RTObject? @object);

    partial class Loader
    {
        public LoadObject LoadObject;

        public IResult Load(RTObject? @object)
            => LoadObject(@object);

        public CompilerObject LoadString(string value)
            => Load(value);

        private IResult DefaultLoader(RTObject? @object)
            => @object switch
            {
                CompilerObject co => Result.Ok(co),
                IResult result => result,
                string value => Result.Ok(Load(value)),
                null => Result.Error("Cannot directly load null object because it is untyped."),
                _ => Result.Error($"Cannot load {@object.GetType().Name} object: {@object}."),
            };
    }
}
