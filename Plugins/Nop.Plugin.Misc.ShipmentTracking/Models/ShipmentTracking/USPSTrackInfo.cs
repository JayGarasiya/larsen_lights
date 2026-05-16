using System.Xml.Linq;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.ShipmentTracking
{
    /// <summary>
    /// Represents a USPS track info
    /// </summary>
    public class USPSTrackInfo
    {
        #region Ctor

        public USPSTrackInfo(string trackId, USPSTrackDetail summary)
        {
            TrackId = trackId;
            TrackSummary = summary;
            TrackDetails = new List<USPSTrackDetail>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Package Tracking identifier 
        /// </summary>
        public string TrackId { get; set; }

        /// <summary>
        /// Tracking Summary Information.
        /// </summary>
        public USPSTrackDetail TrackSummary { get; set; }

        /// <summary>
        /// Tracking Detail Information.
        /// </summary>
        public IList<USPSTrackDetail> TrackDetails { get; set; }

        #endregion

        #region Methods

        public static async Task<USPSTrackInfo> LoadAsync(Stream stream)
        {
            try
            {
                var document = await XDocument.LoadAsync(stream, LoadOptions.None, default);
                var trackInfoElement = document?.Root.Element("TrackInfo");

                if (trackInfoElement == null)
                    return null;

                var trackId = trackInfoElement.Attribute("ID")?.Value ?? string.Empty;
                var trackSummary = trackInfoElement.Element("TrackSummary");
                var track = new USPSTrackInfo(trackId, new USPSTrackDetail(trackSummary));

                foreach (var trackDetail in trackInfoElement.Elements("TrackDetail"))
                {
                    track.TrackDetails.Add(new USPSTrackDetail(trackDetail));
                }

                return track;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
