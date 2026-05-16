using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Services.Common.Pdf;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;

namespace Nop.Plugin.Widgets.ProductExtension.Services
{
    /// <summary>
    /// Represents the cart quote document
    /// </summary>
    public class CartQuoteDocument : PdfDocument<ProductItem>
    {
        #region Utilities

        protected virtual PdfGrid CreateHeader()
        {
            var headerTable = PdfDocumentHelper.BuildPdfGrid(2, DocumentRunDirection);

            var info = PdfDocumentHelper.BuildPdfGrid(1, DocumentRunDirection);
            info.SpacingAfter = 10;

            info.AddCell(BuildPdfPCell(CustomerGuid));
            info.AddCell(BuildHyperLinkCell<CartQuoteDocument>(d => d.StoreUrl, StoreUrl));
            info.AddCell(BuildPdfPCell($"Date: {OrderDateUser}"));

            headerTable.AddCell(
                PdfDocumentHelper.BuildPdfPCell(info, DocumentRunDirection));

            if (LogoData != null && LogoData.Length > 0 && LogoData.Any(b => b != 0))
            {
                var logo = PdfImageHelper.GetITextSharpImageFromByteArray(LogoData);
                headerTable.AddCell(new PdfPCell(logo, fit: true)
                {
                    Border = 0,
                    FixedHeight = 65,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    RunDirection = DocumentRunDirection
                });
            }
            else
            {
                headerTable.AddCell(new PdfPCell { Border = 0 });
            }

            return headerTable;
        }

        protected virtual PdfGrid CreateTotals()
        {
            var totalsTable = PdfDocumentHelper.BuildPdfGrid(1, DocumentRunDirection);

            if (!string.IsNullOrEmpty(Totals.SubTotal))
            {
                var cell = BuildTextCell<InvoiceTotals>(t => t.SubTotal, Totals.SubTotal);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Border = Rectangle.NO_BORDER;
                cell.PaddingRight = 10;         

                totalsTable.AddCell(cell);
            }

            return totalsTable;
        }
        protected virtual PdfGrid CreateFooter(FooterData footerData)
        {
            var footerTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 2, DocumentRunDirection);
           
            footerTable.AddCell(BuildPdfPCell($"- {footerData.CurrentPageNumber} -", collSpan: 2, horizontalAlign: Element.ALIGN_CENTER));

            return footerTable;
        }

        #endregion

        #region Methods

        public override void Generate(Stream pdfStreamOutput)
        {
            Document
                .PagesFooter(footer =>
                {
                    footer.InlineFooter(inlineFooter =>
                    {
                        inlineFooter.FooterProperties(new FooterBasicProperties
                        {
                            PdfFont = footer.PdfFont,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            RunDirection = Language.Rtl ? PdfRunDirection.RightToLeft : PdfRunDirection.LeftToRight
                        });
                        inlineFooter.AddPageFooter(data => CreateFooter(data));
                    });
                })
                .MainTablePreferences(t => t.ColumnsWidthsType(TableColumnWidthType.Relative))
                .MainTableDataSource(ds => ds.StronglyTypedList(Products))
                .MainTableColumns(columns =>
                {
                    columns.AddColumn(c => ConfigureProductColumn(c, p => p.Name, 6));

                    if (ShowSkuInProductList)
                        columns.AddColumn(c => ConfigureProductColumn(c, p => p.Sku, 2));

                    if (ShowVendorInProductList)
                        columns.AddColumn(c => ConfigureProductColumn(c, p => p.VendorName, 2));

                    columns.AddColumn(c => ConfigureProductColumn(c, p => p.Price, 2));
                    columns.AddColumn(c => ConfigureProductColumn(c, p => p.Quantity, 1));
                    columns.AddColumn(c => ConfigureProductColumn(c, p => p.Total, 2));
                })
                .MainTableEvents(events =>
                {
                    events.MainTableCreated(e =>
                    {
                        e.PdfDoc.Add(CreateHeader());
                    });

                    events.MainTableAdded(e =>
                    {
                        e.PdfDoc.Add(CreateTotals());
                    });
                })
                .Generate(builder => builder.AsPdfStream(pdfStreamOutput, closeStream: false));
        }

        #endregion

        #region Properties (same as InvoiceDocument)

        public string CustomerGuid { get; set; }

        public string StoreUrl { get; set; }

        public string OrderDateUser { get; set; }

        public byte[] LogoData { get; set; }

        public bool ShowSkuInProductList { get; set; }

        public bool ShowVendorInProductList { get; set; }

        public InvoiceTotals Totals { get; set; } = new();

        #endregion
    }
}
