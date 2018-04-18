using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ProjectRenamer
{
    static class XmlExtensions
    {
        public static XAttribute GetAttributeByLocalName(this XElement @this, string localName)
        {
            return @this.Attributes().Single(attr => attr.Name.LocalName == localName);
        }

        public static XElement GetDescendantByLocalName(this XContainer @this, string localName)
        {
            return @this.Descendants().Single(attr => attr.Name.LocalName == localName);
        }

        public static IEnumerable<XElement> GetDescendantsByLocalName(this XContainer @this, string localName)
        {
            return @this.Descendants().Where(attr => attr.Name.LocalName == localName);
        }
        public static XElement GetElementByLocalName(this XContainer @this, string localName)
        {
            return @this.Elements().Single(attr => attr.Name.LocalName == localName);
        }
    }
}
