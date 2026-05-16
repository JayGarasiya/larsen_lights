using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Services.Common.Pdf;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using System.ComponentModel;
using System.Globalization;

namespace Nop.Plugin.Widgets.ProductExtension.Domain
{
    /// <summary>
    /// Represents the catalog stock document
    /// </summary>
    public class CatalogStockDocument : PdfDocument<CatalogProductItem>
    {
        #region Utilities

        /// <summary>
        /// Create header
        /// </summary>
        /// <returns></returns>
        protected virtual PdfGrid CreateHeader()
        {
            var headerTable = PdfDocumentHelper.BuildPdfGrid(2, DocumentRunDirection);

            var infoTable = PdfDocumentHelper.BuildPdfGrid(1, DocumentRunDirection);
            infoTable.SpacingAfter = 12;

            infoTable.AddCell(
                BuildHyperLinkCell<CatalogStockDocument>(d => d.StoreUrl, StoreUrl)
            );

            infoTable.AddCell(
                BuildPdfPCell(
                    CreatedOnDateUser.ToString(
                        "D",
                        CultureInfo.GetCultureInfo(Language.LanguageCulture))
                )
            );

            headerTable.AddCell(
                PdfDocumentHelper.BuildPdfPCell(infoTable, DocumentRunDirection)
            );

            if (LogoData != null)
            {
                var logo = PdfImageHelper.GetITextSharpImageFromByteArray(LogoData);
                headerTable.AddCell(new PdfPCell(logo, fit: true)
                {
                    Border = Rectangle.NO_BORDER,
                    FixedHeight = 65,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    RunDirection = DocumentRunDirection
                });
            }
            else
            {
                headerTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
            }

            return headerTable;
        }

        /// <summary>
        /// Create totals
        /// </summary>
        /// <returns></returns>
        protected virtual PdfGrid CreateTotals()
        {
            var totalsTable = PdfDocumentHelper.BuildPdfGrid(1, DocumentRunDirection);
            totalsTable.SpacingBefore = 20;

            if (!string.IsNullOrEmpty(Totals.TotalProductCost))
            {
                var cell = BuildTextCell<CatalogStockTotal>(
                    t => t.TotalProductCost, Totals.TotalProductCost);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Border = Rectangle.NO_BORDER;

                totalsTable.AddCell(cell);
            }

            if (!string.IsNullOrEmpty(Totals.TotalPrice))
            {
                var cell = BuildTextCell<CatalogStockTotal>(
                    t => t.TotalPrice, Totals.TotalPrice);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Border = Rectangle.NO_BORDER;

                totalsTable.AddCell(cell);
            }

            return totalsTable;
        }

        /// <summary>
        /// Build product image cell
        /// </summary>
        /// <param name="data">data</param>
        /// <returns></returns>
        protected virtual PdfPCell BuildProductImageCell(InlineFieldData data)
        {
            var rowData = data.Attributes.RowData.TableRowData;
            var pictureBytes =
                (byte[])rowData.GetValueOf<CatalogProductItem>(x => x.PicturePath);

            const float minRowHeight = 45f;

            if (pictureBytes == null || pictureBytes.Length == 0)
            {
                return new PdfPCell(new Phrase(string.Empty))
                {
                    MinimumHeight = minRowHeight,
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = BaseColor.LightGray,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    PaddingTop = 8,
                    PaddingBottom = 8,
                    RunDirection = DocumentRunDirection
                };
            }

            var image = PdfImageHelper.GetITextSharpImageFromByteArray(pictureBytes);
            image.ScaleToFit(26f, 26f);
            image.Alignment = Element.ALIGN_CENTER;

            return new PdfPCell(image, fit: true)
            {
                MinimumHeight = minRowHeight,
                Border = Rectangle.BOTTOM_BORDER,
                BorderColor = BaseColor.LightGray,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 8,
                PaddingBottom = 8,
                RunDirection = DocumentRunDirection
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate
        /// </summary>
        /// <param name="pdfStreamOutput">pdfStreamOutput</param>
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
                .MainTableColumns(columns =>
                {
                    // IMAGE
                    columns.AddColumn(c =>
                    {
                        ConfigureProductColumn(c, p => p.PicturePath, 2);
                        c.CellsHorizontalAlignment(HorizontalAlignment.Center);

                        c.ColumnItemsTemplate(t =>
                        {
                            t.InlineField(f => f.RenderCell(BuildProductImageCell));
                        });
                    });

                    // NAME
                    columns.AddColumn(c =>
                    {
                        ConfigureProductColumn(c, p => p.Name, 4);
                        c.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    });

                    // SKU
                    columns.AddColumn(c =>
                    {
                        ConfigureProductColumn(c, p => p.Sku, 2);
                        c.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    });

                    // COST
                    columns.AddColumn(c =>
                    {
                        ConfigureProductColumn(c, p => p.ProductCost, 2);
                        c.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    });

                    // PRICE
                    columns.AddColumn(c =>
                    {
                        ConfigureProductColumn(c, p => p.Price, 2);
                        c.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    });

                    // STOCK
                    columns.AddColumn(c =>
                    {
                        ConfigureProductColumn(c, p => p.Stock, 2);
                        c.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    });

                    // TOTAL COST
                    columns.AddColumn(c =>
                    {
                        ConfigureProductColumn(c, p => p.TotalProductCost, 2);
                        c.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    });

                    // TOTAL
                    columns.AddColumn(c =>
                    {
                        ConfigureProductColumn(c, p => p.Total, 3);
                        c.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    });
                })
                .MainTableEvents(events =>
                {
                    events.MainTableCreated(e => e.PdfDoc.Add(CreateHeader()));
                    events.MainTableAdded(e => e.PdfDoc.Add(CreateTotals()));
                })
                .Generate(builder => builder.AsPdfStream(pdfStreamOutput, closeStream: false));
        }

        #endregion

        #region Properties

        public byte[] LogoData { get; set; }

        [DisplayName("Plugins.Widgets.ProductExtension.CatalogStockPdf.CreatedOn")]
        public DateTime CreatedOnDateUser { get; set; }

        public string StoreUrl { get; set; }

        public CatalogStockTotal Totals { get; set; } = new();

        #endregion
    }
}
