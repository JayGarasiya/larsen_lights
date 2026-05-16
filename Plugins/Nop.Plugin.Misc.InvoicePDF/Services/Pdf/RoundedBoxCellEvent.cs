using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Nop.Plugin.Misc.InvoicePDF.Services.Pdf
{
    public class RoundedBoxCellEvent : IPdfPCellEvent
    {
        #region field
        private readonly float _radius;
        private readonly float _borderWidth;
        private readonly BaseColor _borderColor;
        #endregion

        #region ctor 
        public RoundedBoxCellEvent(float radius, float borderWidth, BaseColor borderColor)
        {
            _radius = radius;
            _borderWidth = borderWidth;
            _borderColor = borderColor;
        }
        #endregion

        #region method
        public void CellLayout(PdfPCell cell, Rectangle rect, PdfContentByte[] canvas)
        {
            var cb = canvas[PdfPTable.BACKGROUNDCANVAS];
            cb.SaveState();

            cb.SetLineWidth(_borderWidth);
            cb.SetColorStroke(_borderColor);
            cb.SetColorFill(new BaseColor(245, 245, 245));

            cb.SetLineJoin(PdfContentByte.LINE_JOIN_ROUND);

            cb.RoundRectangle(
                rect.Left + _borderWidth / 2,
                rect.Bottom + _borderWidth / 2,
                rect.Width - _borderWidth,
                rect.Height - _borderWidth,
                _radius
            );

            cb.FillStroke();
            cb.RestoreState();
        }
        #endregion
    }
}
