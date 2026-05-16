using System.Globalization;
using System.Xml.Linq;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represent an USPS tracking detail .
    /// </summary>
    public class USPSTrackDetail
    {
        #region Ctor

        public USPSTrackDetail(XElement trackDetailElement)
        {
            ArgumentNullException.ThrowIfNull(trackDetailElement);

            Event = trackDetailElement.GetValueOfXMLElement("Event");
            City = trackDetailElement.GetValueOfXMLElement("EventCity");
            State = trackDetailElement.GetValueOfXMLElement("EventState");
            ZIPCode = trackDetailElement.GetValueOfXMLElement("EventZIPCode");
            Country = trackDetailElement.GetValueOfXMLElement("EventCountry");
            FirmName = trackDetailElement.GetValueOfXMLElement("FirmName");
            Name = trackDetailElement.GetValueOfXMLElement("Name");

            var eventDate = trackDetailElement.GetValueOfXMLElement("EventDate");
            var eventTime = trackDetailElement.GetValueOfXMLElement("EventTime");

            if (!string.IsNullOrEmpty(eventDate) && !string.IsNullOrEmpty(eventTime))
            {
                Date = DateTime.ParseExact($"{eventDate} {eventTime}", "MMMM d, yyyy h:mm tt", CultureInfo.InvariantCulture);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The event type (e.g., Enroute).
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        ///The city where the event occurred.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The date and time of the event
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The state where the event occurred.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The ZIP Code of the event.
        /// </summary>
        public string ZIPCode { get; set; }

        /// <summary>
        /// The country where the event occurred.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// The company name if delivered to a company.
        /// </summary>
        public string FirmName { get; set; }

        /// <summary>
        /// The name of the persons signing for delivery (if available).
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}
