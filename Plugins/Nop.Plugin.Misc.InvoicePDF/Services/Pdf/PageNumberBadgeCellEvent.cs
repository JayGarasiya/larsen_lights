using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Nop.Plugin.Misc.InvoicePDF.Services.Pdf
{
    /// <summary>
    /// Get Page Number Badge Cell Event
    /// </summary>
    public class PageNumberBadgeCellEvent : IPdfPCellEvent
    {
        #region field
        private readonly int _pageNumber;
        #endregion

        #region ctor
        public PageNumberBadgeCellEvent(int pageNumber)
        {
            _pageNumber = pageNumber;
        }
        #endregion

        #region method

        public void CellLayout(PdfPCell cell, Rectangle rect, PdfContentByte[] canvases)
        {
            var canvas = canvases[PdfPTable.BACKGROUNDCANVAS];

            // Oval dimensions
            float width = 55;
            float height = 35;

            // Center the badge in the cell
            float centerX = rect.Left + rect.Width / 2;
            float centerY = rect.Bottom + rect.Height / 2;
            float x = centerX - width / 2;
            float y = centerY - height / 2;

            canvas.SaveState();

            // Draw three concentric oval borders with no gaps
            // Outer border - Dark Blue (6, 47, 79)
            float borderWidth1 = 2f;
            DrawOvalBorder(canvas, x, y, width, height, new BaseColor(6, 47, 79), borderWidth1, 18f);

            // Middle border - Gold (200, 187, 115)
            float borderWidth2 = 1.5f;
            float offset1 = borderWidth1;
            DrawOvalBorder(canvas, x + offset1, y + offset1, width - (offset1 * 2), height - (offset1 * 2), new BaseColor(200, 187, 115), borderWidth2, 16f);

            // Inner border - Red (184, 38, 1)
            float borderWidth3 = 1.5f;
            float offset2 = offset1 + borderWidth2;
            DrawOvalBorder(canvas, x + offset2, y + offset2, width - (offset2 * 2), height - (offset2 * 2), new BaseColor(184, 38, 1), borderWidth3, 14f);

            canvas.RestoreState();

            // Draw the page number text in the center
            var textCanvas = canvases[PdfPTable.TEXTCANVAS];
            ColumnText.ShowTextAligned(
                textCanvas,
                Element.ALIGN_CENTER,
                new Phrase(_pageNumber.ToString(), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.Black)),
                centerX,
                centerY - 4f,
                0
            );
        }

        private void DrawOvalBorder(PdfContentByte cb, float x, float y, float width, float height, BaseColor color, float lineWidth, float radius)
        {
            cb.SetColorStroke(color);
            cb.SetLineWidth(lineWidth);
            cb.RoundRectangle(x, y, width, height, radius);
            cb.Stroke();
        }
        #endregion
    }
}