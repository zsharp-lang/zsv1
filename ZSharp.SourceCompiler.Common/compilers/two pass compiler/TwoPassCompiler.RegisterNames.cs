namespace ZSharp.SourceCompiler
{
    partial class TwoPassCompiler
    {
        private void RegisterNames()
        {
            // First thing we do is iterate through all the items

            // But in reality, we don't really care about statement here
            // We only care about definitions, wherever they appear.

            // So we shouldn't even bother with a list of defintions.
            // We can just use expose an API to add a definition.
            // The thing is, we can't allow adding definitions after
            // the first pass is complete.

            // So we either hold a state of the current pass or we can
            // separate the process into two separate classes.
            // This way there's a clear data flow and no hidden state.
        }
    }
}
