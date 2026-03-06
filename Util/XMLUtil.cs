using System.Threading.Tasks;
using System.Xml;

namespace RDEditorPlus.Util
{
    public static class XMLUtil
    {
        public static Task WriteStartElementAsync(this XmlWriter writer, string localName)
        {
            return writer.WriteStartElementAsync(string.Empty, localName, string.Empty);
        }

        public static Task WriteElementStringAsync(this XmlWriter writer, string localName, string value)
        {
            return writer.WriteElementStringAsync(string.Empty, localName, string.Empty, value);
        }

        public static Task WriteAttributeStringAsync(this XmlWriter writer, string localName, string value)
        {
            return writer.WriteAttributeStringAsync(string.Empty, localName, string.Empty, value);
        }
    }
}
