using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Services.Common.Pdf;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Language = Nop.Core.Domain.Localization.Language;

namespace Nop.Plugin.Misc.InvoicePDF.Services.Pdf
{
    /// <summary>
    /// Represents base document class
    /// </summary>
    public abstract class OverridePdfDocument<TItem>
    {
        #region Utilities

        /// <summary>
        /// Build a cell with a hyperlink 
        /// </summary>
        /// <param name="labelSelector">Property selector to get resource key annotation</param>
        /// <param name="url">URL</param>
        /// <returns>A cell for PDF table</returns>
        protected virtual PdfPCell BuildHyperLinkCell<TLabel>(Expression<Func<TLabel, string>> labelSelector, string url)
        {
            ArgumentNullException.ThrowIfNull(labelSelector);
            ArgumentNullException.ThrowIfNullOrEmpty(url);

            var content = new Phrase();
            var label = LabelField(labelSelector, Font, Language);

            if (label.IsEmpty())
                label.Append(url);

            content.Add(new Anchor(label) { Reference = url });

            var cell = new PdfPCell(content)
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                RunDirection = DocumentRunDirection,
                Border = 0,
                Padding = 3
            };

            cell.SetLeading(0f, PdfDocumentHelper.RELATIVE_LEADING);

            return cell;
        }

        /// <summary>
        /// Build a cell with the given property
        /// </summary>
        /// <param name="labelSelector">Property selector to get resource key annotation</param>
        /// <param name="value">Value to format</param>
        /// <param name="horizontalAlign">Horizontal alignment</param>
        /// <returns>A cell for PDF table</returns>
        protected virtual PdfPCell BuildPdfPCell<TLabel>(Expression<Func<TLabel, string>> labelSelector, string value, int horizontalAlign = Element.ALIGN_LEFT)
        {
            var label = LabelField(labelSelector, Font, Language, value);

            var cell = new PdfPCell(new Phrase() { label })
            {
                RunDirection = DocumentRunDirection,
                HorizontalAlignment = horizontalAlign,
                VerticalAlignment = Element.ALIGN_CENTER,
                Border = 0,
                Padding = 3
            };

            cell.SetLeading(0f, PdfDocumentHelper.RELATIVE_LEADING);

            return cell;
        }

        /// <summary>
        /// Build a cell with the given text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="collSpan">The number of columns occupied by a cell</param>
        /// <param name="horizontalAlign">Horizontal alignment</param>
        /// <param name="verticalAlignment">Vertical alignment</param>
        /// <returns>A cell for PDF table</returns>
        protected virtual PdfPCell BuildPdfPCell(string text, int collSpan = 1, int horizontalAlign = Element.ALIGN_LEFT, int verticalAlignment = Element.ALIGN_CENTER)
        {
            var cell = new PdfPCell(new Phrase(text, Font))
            {
                HorizontalAlignment = horizontalAlign,
                VerticalAlignment = verticalAlignment,
                Colspan = collSpan,
                RunDirection = DocumentRunDirection,
                Border = 0,
                Padding = 3
            };

            cell.SetLeading(0f, PdfDocumentHelper.RELATIVE_LEADING);

            return cell;
        }

        /// <summary>
        /// Build a cell for the given selector and text
        /// </summary>
        /// <param name="labelSelector">Property selector to get resource key annotation</param>
        /// <param name="text">Text</param>
        /// <returns>A cell for PDF table</returns>
        protected virtual PdfPCell BuildTextCell<TLabel>(Expression<Func<TLabel, string>> labelSelector, string text)
        {
            ArgumentNullException.ThrowIfNull(labelSelector);
            ArgumentNullException.ThrowIfNullOrEmpty(text);

            var label = LabelField(labelSelector, Font, Language);

            var content = new Phrase() { label, new Chunk(":", Font), new Chunk(" ", Font), new Chunk(text, Font) };
            var cell = new PdfPCell(content)
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                RunDirection = DocumentRunDirection,
                Border = 0,
                Padding = 3
            };

            cell.SetLeading(0f, PdfDocumentHelper.RELATIVE_LEADING);

            return cell;
        }

        /// <summary>
        /// Build a table for address item
        /// </summary>
        /// <param name="labelSelector">Property selector to get resource key annotation</param>
        /// <param name="address">Address item</param>
        /// <returns>PDF table</returns>
        protected virtual PdfGrid BuildAddressTable<TLabel>(Expression<Func<TLabel, AddressItem>> labelSelector, AddressItem address)
        {
            var table = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);
            table.WidthPercentage = 100;

            var title = LabelField(labelSelector, Font, Language);
            table.AddCell(new PdfPCell(new Phrase(title))
            {
                Border = 0,
                PaddingBottom = 6
            });

            var divider = new PdfPCell { Border = 0, PaddingBottom = 8 };
            divider.AddElement(new LineSeparator(1f, 100f, new BaseColor(6, 47, 79), Element.ALIGN_LEFT, 0));
            table.AddCell(divider);

            void AddText(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                table.AddCell(new PdfPCell(new Phrase(value, Font))
                {
                    Border = 0,
                    PaddingBottom = 3
                });
            }

            AddText(address.Name);
            AddText(address.Company);
            AddText(address.AddressLine);

            if (!string.IsNullOrWhiteSpace(address.Phone))
                AddText($"Phone: {address.Phone}");

            if (!string.IsNullOrWhiteSpace(address.VATNumber))
                AddText($"VAT: {address.VATNumber}");

            if (address.AddressAttributes != null)
            {
                foreach (var attribute in address.AddressAttributes)
                    AddText(attribute);
            }

            if (!string.IsNullOrEmpty(address.PaymentMethod))
            {
                var grayLine = new PdfPCell { Border = 0, PaddingTop = 6, PaddingBottom = 6 };
                grayLine.AddElement(new LineSeparator(1f, 100f, new BaseColor(6, 47, 79), Element.ALIGN_LEFT, 0));
                table.AddCell(grayLine);

                AddText($"Payment method: {address.PaymentMethod}");
            }

            if (!string.IsNullOrEmpty(address.ShippingMethod))
            {
                var grayLine = new PdfPCell { Border = 0, PaddingTop = 6, PaddingBottom = 6 };
                grayLine.AddElement(new LineSeparator(1f, 100f, new BaseColor(6, 47, 79), Element.ALIGN_LEFT, 0));
                table.AddCell(grayLine);

                AddText($"Shipping method: {address.ShippingMethod}");
            }

            if (address.CustomValues != null)
            {
                foreach (var kv in address.CustomValues)
                {
                    AddText($"{kv.Name}: {kv.Value}");
                }
            }

            return table;
        }

        /// <summary>
        /// Specify default behavior for maintable column
        /// </summary>
        /// <param name="column">Column builder</param>
        /// <param name="propertyExpression">Property selector for cells in the column</param>
        /// <param name="width">The column's width according to the PdfRptPageSetup.MainTableColumnsWidthsType value</param>
        /// <param name="printProductAttributes">Indicates that product attribute descriptions should be printed if they exist</param>
        //protected virtual void ConfigureProductColumn(ColumnAttributesBuilder column, Expression<Func<TItem, object>> propertyExpression, int width = 1, bool printProductAttributes = false)
        //{
        //    column.PropertyName(propertyExpression);
        //    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
        //    column.IsVisible(true);
        //    column.Width(width);
        //    column.HeaderCell(LabelField(propertyExpression, Font, Language).Content, horizontalAlignment: HorizontalAlignment.Left);

        //    column.ColumnItemsTemplate(itemsTemplate =>
        //    {
        //        itemsTemplate.InlineField(inlineField =>
        //        {
        //            inlineField.RenderCell(cellData =>
        //            {
        //                var table = new PdfGrid(numColumns: 1)
        //                {
        //                    WidthPercentage = 100,
        //                    RunDirection = DocumentRunDirection,
        //                    HorizontalAlignment = Element.ALIGN_LEFT,
        //                    SpacingAfter = 5,
        //                    SpacingBefore = 5
        //                };

        //                var data = cellData.Attributes.RowData.TableRowData;
        //                var text = data.GetSafeStringValueOf(propertyExpression);

        //                table.AddCell(BuildPdfPCell(text, verticalAlignment: Element.ALIGN_TOP));

        //                if (printProductAttributes)
        //                {
        //                    var productAttributes = (List<string>)data.GetValueOf((ProductItem x) => x.ProductAttributes);
        //                    var font8Italic = PdfDocumentHelper.GetFont(Font, Font.Size * 0.8f, DocumentFontStyle.Italic);

        //                    foreach (var pa in productAttributes)
        //                    {
        //                        table.AddCell(new PdfPCell(new Phrase(pa, font8Italic))
        //                        {
        //                            RunDirection = DocumentRunDirection,
        //                            HorizontalAlignment = Element.ALIGN_LEFT,
        //                            Border = 0
        //                        });
        //                    }
        //                }

        //                return new PdfPCell(table)
        //                {
        //                    RunDirection = DocumentRunDirection,
        //                    BorderWidthBottom = 2,
        //                    BorderColorBottom = new BaseColor(224, 224, 224),
        //                    MinimumHeight = 25,
        //                    VerticalAlignment = Element.ALIGN_CENTER
        //                };
        //            });
        //        });
        //    });
        //}

        protected virtual void ConfigureProductColumn(ColumnAttributesBuilder column, Expression<Func<TItem, object>> propertyExpression, int width = 1, bool printProductAttributes = false)
        {
            column.PropertyName(propertyExpression);
            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
            column.IsVisible(true);
            column.Width(width);
            column.HeaderCell(LabelField(propertyExpression, Font, Language).Content, horizontalAlignment: HorizontalAlignment.Left);

            column.ColumnItemsTemplate(itemsTemplate =>
            {
                itemsTemplate.InlineField(inlineField =>
                {
                    inlineField.RenderCell(cellData =>
                    {
                        var table = new PdfGrid(numColumns: 1)
                        {
                            WidthPercentage = 100,
                            RunDirection = DocumentRunDirection,
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            SpacingAfter = 5,
                            SpacingBefore = 5
                        };

                        var data = cellData.Attributes.RowData.TableRowData;
                        var text = data.GetSafeStringValueOf(propertyExpression);

                        table.AddCell(BuildPdfPCell(text, verticalAlignment: Element.ALIGN_TOP));

                        if (printProductAttributes)
                        {
                            var productAttributes = (List<string>)data.GetValueOf((ProductItem x) => x.ProductAttributes);
                            var font8Italic = PdfDocumentHelper.GetFont(Font, Font.Size * 0.8f, DocumentFontStyle.Italic);

                            foreach (var pa in productAttributes)
                            {
                                table.AddCell(new PdfPCell(new Phrase(pa, font8Italic))
                                {
                                    RunDirection = DocumentRunDirection,
                                    HorizontalAlignment = Element.ALIGN_LEFT,
                                    Border = 0
                                });
                            }
                        }

                        return new PdfPCell(table)
                        {
                            RunDirection = DocumentRunDirection,
                            BorderWidthBottom = 2,
                            BorderColorBottom = new BaseColor(220, 220, 220),
                            MinimumHeight = 25,
                            VerticalAlignment = Element.ALIGN_CENTER
                        };
                    });
                });
            });
        }

        /// <summary>
        /// Get default document builder
        /// </summary>
        /// <returns>PDF document builder</returns>
        protected virtual PdfReport DefaultDocument()
        {
            var pdfSettings = EngineContext.Current.Resolve<PdfSettings>();
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var fontSize = pdfSettings.BaseFontSize >= 0 ? (int)pdfSettings.BaseFontSize : 10;
            var fontFileName = Language.Rtl ? pdfSettings.RtlFontName : pdfSettings.LtrFontName;

            var mainFontPath = fileProvider.Combine(fileProvider.MapPath("~/App_Data/Pdf/"), $"{fontFileName}.ttf");

            return new PdfReport()
                .DocumentPreferences(doc =>
                {
                    doc.RunDirection(Language.Rtl ? PdfRunDirection.RightToLeft : PdfRunDirection.LeftToRight);
                    doc.Orientation(PageOrientation.Portrait);
                    doc.PageSize(PageSize);
                    doc.DocumentMargins(new DocumentMargins { Left = 100, Right = 100, Top = 100, Bottom = 100 });
                })
                .DefaultFonts(fonts =>
                {
                    fonts.Color(System.Drawing.Color.Black);
                    fonts.Size(10);
                    fonts.Path(mainFontPath, mainFontPath);
                })
                .MainTableEvents(events =>
                {
                    events.CellCreated(args =>
                    {
                        if (args.CellType == CellType.HeaderCell && !string.IsNullOrWhiteSpace(args.Cell.RowData.Value?.ToString()))
                        {
                            args.Cell.BasicProperties.BackgroundColor = new BaseColor(245, 245, 245);
                            args.Cell.BasicProperties.CellPadding = 5;
                            args.Cell.BasicProperties.BorderColor = new BaseColor(6, 47, 79);
                            args.Cell.BasicProperties.BorderWidth = 1;
                            args.Cell.BasicProperties.ShowBorder = true;
                        }
                    });
                });
        }

        /// <summary>
        /// Get a label for the given property
        /// </summary>
        /// <param name="propertyExpression">Property selector to get resource key annotation</param>
        /// <param name="font">Font</param>
        /// <param name="language">Language</param>
        /// <param name="args">Array of objects to format the resource string</param>
        /// <returns>A chunk with localized annotation if present, otherwise an empty chunk</returns>
        protected virtual Chunk LabelField<TLabel, TOut>(Expression<Func<TLabel, TOut>> propertyExpression, Font font, Language language, params string[] args)
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var propertyInfo = (PropertyInfo)expression.Member;

            var label = propertyInfo.GetCustomAttributes<DisplayNameAttribute>(true).FirstOrDefault() is DisplayNameAttribute attr
                ? GetResourceAsync(attr.DisplayName, language?.Id ?? 0).Result
                : string.Empty;

            if (!string.IsNullOrEmpty(label) && args.Any())
                label = string.Format(label, args);

            return new Chunk(label, font);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate document
        /// </summary>
        /// <param name="pdfStreamOutput">Stream for PDF output</param>
        public abstract void Generate(Stream pdfStreamOutput);

        #endregion

        #region Properties

        /// <summary>
        /// PDF document builder 
        /// </summary>
        protected PdfReport Document => DefaultDocument();

        /// <summary>
        /// Gets or sets a collection of items
        /// </summary>
        public List<TItem> Products { get; init; }

        /// <summary>
        /// Gets or sets the language context
        /// </summary>
        public required Language Language { get; init; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public required PdfPageSize PageSize { get; init; }

        /// <summary>
        /// Gets or sets the font name. Loaded from the ~/App_Data/Pdf directory during application start.
        /// </summary>
        public required Font Font { get; init; }

        /// <summary>
        /// Gets or sets the size required to scale images before rendering
        /// </summary>
        public required int ImageTargetSize { get; init; }

        /// <summary>
        /// Gets document run direction
        /// </summary>
        public int DocumentRunDirection => Language?.Rtl == true ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;

        /// <summary>
        /// Gets or sets a function to get a resource string by the specified key and language identifier
        /// </summary>
        public required Func<string, int, Task<string>> GetResourceAsync { get; init; }

        #endregion
    }
}
