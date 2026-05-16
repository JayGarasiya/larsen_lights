using Nop.Core;
using System.Xml.Linq;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represents a XElement extensions
    /// </summary>
    public static class XElementExtensions
    {
        #region Methods

        public static string GetValueOfXMLElement(this XElement element, string elementName)
        {
            return element.GetValueOfXMLElement<string>(elementName);
        }


        public static T GetValueOfXMLElement<T>(this XElement element, string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
                throw new ArgumentNullException(nameof(elementName));

            return CommonHelper.To<T>(element.Element(elementName)?.Value);
        }

        public static string GetValueOfXMLAttribute(this XElement element, string attributeName)
        {
            return element.GetValueOfXMLAttribute<string>(attributeName);
        }


        public static T GetValueOfXMLAttribute<T>(this XElement element, string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName))
                throw new ArgumentNullException(nameof(attributeName));

            return CommonHelper.To<T>(element.Attribute(attributeName)?.Value);
        }

        #endregion
    }
}
