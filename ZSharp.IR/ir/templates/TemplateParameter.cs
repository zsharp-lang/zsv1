namespace ZSharp.IR
{
    public sealed class TemplateParameter
    {
        public string Name { get; set; }

        public TemplateParameter(string name)
        {
            Name = name;
        }
    }
}
