using System.Collections;

namespace RDEditorPlus.Functionality.Mixins
{
    public interface IAutocompletionStorage
    {
        internal PluginConfig.AutocompleteBehaviour Behaviour { get; }
        public string Identifier { get; }
        public string URL { get; }
        public string PlayerSuppliedFile { get; }
        public string TemporaryAutodownloadFile { get; }
        public int RefreshTime { get; }

        public IEnumerator Parse(string text);
    }
}
