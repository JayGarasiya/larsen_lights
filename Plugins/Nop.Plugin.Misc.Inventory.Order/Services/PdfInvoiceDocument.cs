using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Plugin.Misc.Inventory.Order.Models;
using Nop.Services.Common.Pdf;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using System.ComponentModel;

namespace Nop.Plugin.Misc.Inventory.Order.Services
{
    public partial class PdfInvoiceDocument : PdfDocument<PdfProductItems>
    {
        #region Utilities

        protected virtual PdfGrid CreateInvoiceHeader()
        {
            var headerTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 2, DocumentRunDirection);

            var info = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);
            info.SpacingAfter = 15;

            info.AddCell(BuildTextCell<PdfInvoiceDocument>(source => OrderNumberText, OrderNumberText));
            info.AddCell(BuildHyperLinkCell<PdfInvoiceDocument>(source => StoreUrl, StoreUrl));
            info.AddCell(BuildTextCell<PdfInvoiceDocument>(source => CompanyAddress, CompanyAddress));
            info.AddCell(BuildTextCell<PdfInvoiceDocument>(source => OrderDateUser, OrderDateUser));
            info.AddCell(BuildTextCell<PdfInvoiceDocument>(source => OrderComment, OrderComment));
            info.AddCell(BuildTextCell<PdfInvoiceDocument>(source => TotalVolume, TotalVolume));

            headerTable.AddCell(PdfDocumentHelper.BuildPdfPCell(info, DocumentRunDirection, horizontalAlign: Element.ALIGN_LEFT));

            if (LogoData is not null)
            {
                var logo = PdfImageHelper.GetITextSharpImageFromByteArray(LogoData);
                headerTable.AddCell(new PdfPCell(logo, fit: true)
                {
                    Border = 0,
                    FixedHeight = 60,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }
            else
            {
                headerTable.AddCell(new PdfPCell(new Phrase())
                {
                    Border = 0,
                    Padding = 0
                });
            }

            return headerTable;
        }

        protected virtual PdfGrid CreateFooter(FooterData footerData)
        {
            var footer = PdfDocumentHelper.BuildPdfGrid(1, DocumentRunDirection);

            footer.AddCell(new PdfPCell(new Phrase($"- {footerData.CurrentPageNumber} -"))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingTop = 10
            });

            return footer;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate the invoice
        /// </summary>
        /// <param name="pdfStreamOutput">Stream for PDF output</param>
        public override void Generate(Stream pdfStreamOutput)
        {
            Document
                .MainTablePreferences(table =>
                {
                    table.ColumnsWidthsType(TableColumnWidthType.Relative);
                })
                .MainTableDataSource(dataSource =>
                {
                    dataSource.StronglyTypedList(Products);
                })
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
                .MainTableColumns(columns =>
                {
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.ManufacturerName, width: 3));
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.ProductName, width: 10));
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.SKU, width: 3));
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.ProductCost, width: 3));
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.QTY, width: 2));
                })
                .MainTableEvents(events =>
                {
                    events.MainTableCreated(events =>
                    {
                        events.PdfDoc.Add(CreateInvoiceHeader());
                    });
                    events.MainTableAdded(events =>
                    {
                        var summaryTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 3, DocumentRunDirection);
                        summaryTable.AddCell(new PdfPCell() { Colspan = 2, Border = 0 });
                        events.PdfDoc.Add(summaryTable);
                    });
                })
                .Generate(builder => builder.AsPdfStream(pdfStreamOutput, closeStream: false));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets store location
        /// </summary>
        public string StoreUrl { get; init; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        [DisplayName("Pdf.OrderDate")]
        public required string OrderDateUser { get; init; }

        /// <summary>
        /// Gets or sets the logo binary
        /// </summary>
        public byte[] LogoData { get; set; }

        /// <summary>
        /// Gets or sets the company address
        /// </summary>
        [DisplayName("Pdf.CompanyAddress")]
        public string CompanyAddress { get; set; }

        /// <summary>
        /// Gets or sets the order number
        /// </summary>
        [DisplayName("Pdf.PoOrder")]
        public string OrderNumberText { get; init; }

        /// <summary>
        /// Gets or sets the order comment
        /// </summary>
        [DisplayName("Pdf.OrderComment")]
        public string OrderComment { get; set; }

        /// <summary>
        /// Gets or sets the total volume
        /// </summary>
        [DisplayName("Pdf.TotalVolume")]
        public string TotalVolume { get; set; }

        /// <summary>
        /// Gets or sets the text that will appear at the bottom of invoice (column 1)
        /// </summary>
        public List<string> FooterTextColumn1 { get; set; } = new();

        /// <summary>
        /// Gets or sets the text that will appear at the bottom of invoice (column 2)
        /// </summary>
        public List<string> FooterTextColumn2 { get; set; } = new();

        #endregion
    }
}
