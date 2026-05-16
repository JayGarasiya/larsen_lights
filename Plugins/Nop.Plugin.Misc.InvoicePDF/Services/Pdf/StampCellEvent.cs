using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfRpt.Core.Helper;

namespace Nop.Plugin.Misc.InvoicePDF.Services.Pdf
{
    public class StampCellEvent : IPdfPCellEvent
    {
        #region field
        private readonly byte[] _imageData;
        #endregion

        #region ctor
        public StampCellEvent(byte[] imageData)
        {
            
            _imageData = imageData;
            
        }
        #endregion

        #region method
        public void CellLayout(PdfPCell cell, Rectangle position, PdfContentByte[] canvases)
        {
           
            try
            {
                var canvas = canvases[PdfPTable.BACKGROUNDCANVAS];

                // Load and prepare the image
                var image = PdfImageHelper.GetITextSharpImageFromByteArray(_imageData);
                image.ScaleToFit(140f, 90f);

                // Calculate position (center-right area, over shipping/tax rows)
                float x = position.Right - image.ScaledWidth - 20;
                float y = position.Top - (position.Height * 0.55f) - (image.ScaledHeight / 2);

                // Rotate the image 15 degrees
                float radians = 12f * (float)Math.PI / 180f;
                float cos = (float)Math.Cos(radians);
                float sin = (float)Math.Sin(radians);

                // Save the canvas state
                canvas.SaveState();

                // Apply rotation transformation
                canvas.ConcatCtm(cos, sin, -sin, cos, x, y);

                // Add the image at (0,0) since we've already transformed the canvas
                image.SetAbsolutePosition(0, 0);
                canvas.AddImage(image);

                // Restore the canvas state
                canvas.RestoreState();
            }
            catch (Exception ex)
            {
                // Log or handle error if needed
                System.Diagnostics.Debug.WriteLine($"Error adding PAID stamp: {ex.Message}");
            }
        }
        #endregion
    }
}
