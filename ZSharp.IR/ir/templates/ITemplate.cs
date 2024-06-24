using CommonZ.Utils;

namespace ZSharp.IR
{
    public interface ITemplate
    {
        public IRObject Construct(Mapping<TemplateParameter, IRObject> args);
    }
}
