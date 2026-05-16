using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core.Infrastructure;
using Nop.Services.Common.Pdf;
using Nop.Services.Localization;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using System.ComponentModel;

namespace Nop.Plugin.Misc.InvoicePDF.Services.Pdf
{
    /// <summary>
    /// Represents the invoice document
    /// </summary>
    public class CustomInvoiceDocument : OverridePdfDocument<ProductItem>
    {
        #region Utilities

        protected virtual PdfGrid CreateAdressesInfo()
        {
            var hasShipping = !string.IsNullOrEmpty(ShippingAddress?.ShippingMethod);

            var mainContainer = PdfDocumentHelper.BuildPdfGrid(1, DocumentRunDirection);
            mainContainer.WidthPercentage = 100;

            // Create table with columns for: left fold logo, billing, spacer, shipping, right fold logo
            var innerTable = PdfDocumentHelper.BuildPdfGrid(hasShipping ? 5 : 3, DocumentRunDirection);
            innerTable.WidthPercentage = 100;

            if (hasShipping)
                innerTable.SetWidths(new float[] { 3, 46, 2, 46, 3 });
            else
                innerTable.SetWidths(new float[] { 3, 94, 3 });

            // LEFT FOLD LOGO
            var leftFoldCell = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = BaseColor.White,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 85,
                PaddingBottom = 0,
                PaddingLeft = 0,
                PaddingRight = 15,
            };

            if (FoldLogoData != null)
            {
                var foldImg = PdfImageHelper.GetITextSharpImageFromByteArray(FoldLogoData);
                foldImg.ScaleToFit(10f, 10f);
                leftFoldCell.AddElement(foldImg);
            }
            else
            {
                leftFoldCell.AddElement(new Phrase(" "));
            }

            innerTable.AddCell(leftFoldCell);

            // BILLING ADDRESS
            var billingTable = BuildAddressTable<InvoiceDocument>(x => BillingAddress, BillingAddress);
            var billingCell = new PdfPCell(billingTable)
            {
                Border = Rectangle.NO_BORDER,
                PaddingLeft = 10,
                PaddingRight = 10,
                PaddingTop = 15,
                PaddingBottom = 0,
                CellEvent = new RoundedBoxCellEvent(
                    radius: 14f,
                    borderWidth: 1.5f,
                    borderColor: new BaseColor(245, 245, 245)
                )
            };

            innerTable.AddCell(billingCell);

            if (hasShipping)
            {
                // SPACER
                innerTable.AddCell(new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    BackgroundColor = BaseColor.White
                });

                // SHIPPING ADDRESS
                var shippingTable = BuildAddressTable<InvoiceDocument>(x => ShippingAddress, ShippingAddress);
                var shippingCell = new PdfPCell(shippingTable)
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingLeft = 10,
                    PaddingRight = 10,
                    PaddingTop = 15,
                    PaddingBottom = 0,
                    CellEvent = new RoundedBoxCellEvent(
                        radius: 14f,
                        borderWidth: 1.5f,
                        borderColor: new BaseColor(245, 245, 245)
                    )
                };

                innerTable.AddCell(shippingCell);

                // RIGHT FOLD LOGO
                var rightFoldCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    BackgroundColor = BaseColor.White,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    PaddingTop = 85,
                    PaddingBottom = 0,
                    PaddingLeft = 15,
                    PaddingRight = 0
                };

                if (FoldLogoData != null)
                {
                    var foldImg = PdfImageHelper.GetITextSharpImageFromByteArray(FoldLogoData);
                    foldImg.ScaleToFit(10f, 10f);
                    rightFoldCell.AddElement(foldImg);
                }
                else
                {
                    rightFoldCell.AddElement(new Phrase(" "));
                }

                innerTable.AddCell(rightFoldCell);
            }
            else
            {
                // RIGHT FOLD LOGO (for single column layout)
                var rightFoldCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    BackgroundColor = BaseColor.White,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    PaddingTop = 85,
                    PaddingBottom = 0,
                    PaddingLeft = 15,
                    PaddingRight = 0
                };

                if (FoldLogoData != null)
                {
                    var foldImg = PdfImageHelper.GetITextSharpImageFromByteArray(FoldLogoData);
                    foldImg.ScaleToFit(10f, 10f);
                    rightFoldCell.AddElement(foldImg);
                }
                else
                {
                    rightFoldCell.AddElement(new Phrase(" "));
                }

                innerTable.AddCell(rightFoldCell);
            }

            // ---------------- WHITE PAGE CONTAINER ----------------
            var whiteBackgroundCell = new PdfPCell(innerTable)
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = BaseColor.White,
                PaddingTop = 30,
                PaddingBottom = 15,
                PaddingLeft = 0,
                PaddingRight = 9
            };

            mainContainer.AddCell(whiteBackgroundCell);

            return mainContainer;
        }

        protected virtual PdfGrid CreateInvoiceHeader()
        {
            // Main container
            var mainTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);
            mainTable.WidthPercentage = 100;
            mainTable.SpacingAfter = 0;
            mainTable.SpacingBefore = 0;

            #region Top Header Section

            var topSection = PdfDocumentHelper.BuildPdfGrid(numColumns: 3, DocumentRunDirection);
            topSection.WidthPercentage = 100;
            topSection.SetWidths(new float[] { 10, 45, 45 });

            // Logo cell (LEFT)
            var logoCell = new PdfPCell
            {
                Border = 0,
                PaddingLeft = 20,
                PaddingTop = 0,
                PaddingBottom = 0,
                FixedHeight = 43,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            if (LogoData != null)
            {
                var logo = PdfImageHelper.GetITextSharpImageFromByteArray(LogoData);
                logo.ScaleToFit(174f, 55f);

                var chunk = new Chunk(logo, 0, 0, true);
                chunk.SetAnchor(StoreUrl);

                logoCell.AddElement(chunk);
            }
            else
            {
                logoCell.AddElement(new Phrase(" "));
            }

            topSection.AddCell(logoCell);

            topSection.AddCell(new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(245, 245, 245)
            });

            // Invoice banner (RIGHT)
            var invoiceBannerCell = new PdfPCell
            {
                Border = 0,
                Padding = 0,
                FixedHeight = 43,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            var bannerTable = new PdfPTable(2);
            bannerTable.WidthPercentage = 100;
            bannerTable.SetWidths(new float[] { 10, 90 });

            // Stripes
            var stripesTable = new PdfPTable(3);
            stripesTable.WidthPercentage = 100;
            stripesTable.SetWidths(new float[] { 4, 2, 5 });

            stripesTable.AddCell(new PdfPCell { Border = 0, BackgroundColor = new BaseColor(184, 38, 1), FixedHeight = 43, PaddingLeft = 0, PaddingRight = 0, PaddingTop = 0, PaddingBottom = 0 });
            stripesTable.AddCell(new PdfPCell { Border = 0, BackgroundColor = new BaseColor(200, 187, 115), FixedHeight = 43, PaddingLeft = 0, PaddingRight = 0, PaddingTop = 0, PaddingBottom = 0 });
            stripesTable.AddCell(new PdfPCell { Border = 0, BackgroundColor = new BaseColor(6, 47, 79), FixedHeight = 43, PaddingLeft = 0, PaddingRight = 0, PaddingTop = 0, PaddingBottom = 0 });

            bannerTable.AddCell(new PdfPCell(stripesTable) { Border = 0, Padding = 0 });

            // INVOICE text
            var invoiceFont = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 40, Font.NORMAL, BaseColor.White);
            var invoiceTextCell = new PdfPCell(new Phrase("I N V O I C E", invoiceFont))
            {
                Border = 0,
                BackgroundColor = new BaseColor(6, 47, 79),
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingLeft = 0,
                PaddingRight = 6,
                PaddingTop = 0,
                PaddingBottom = 10
            };

            bannerTable.AddCell(invoiceTextCell);
            invoiceBannerCell.AddElement(bannerTable);
            topSection.AddCell(invoiceBannerCell);

            mainTable.AddCell(new PdfPCell(topSection)
            {
                Border = 0,
                PaddingTop = 20,
                PaddingBottom = 0,
                PaddingLeft = 0,
                PaddingRight = 0,
                BackgroundColor = new BaseColor(245, 245, 245),
            });

            #endregion

            #region Bottom Info Section

            var bottomSection = PdfDocumentHelper.BuildPdfGrid(numColumns: 3, DocumentRunDirection);
            bottomSection.WidthPercentage = 100;
            bottomSection.SetWidths(new float[] { 38, 32, 30 });

            var normalFont = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 10);
            var labelFont = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 10);

            PdfPCell BuildInfoCell(byte[] iconData, Phrase text, float iconPaddingTop = 0)
            {
                var table = new PdfPTable(2);
                table.WidthPercentage = 150;
                table.SetWidths(new float[] { 8, 100 });

                if (iconData != null)
                {
                    var img = PdfImageHelper.GetITextSharpImageFromByteArray(iconData);
                    img.ScaleToFit(25f, 25f);
                    table.AddCell(new PdfPCell(img, true)
                    {
                        Border = 0,
                        PaddingRight = 0,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        PaddingTop = iconPaddingTop,
                    });
                }
                else
                {
                    table.AddCell(new PdfPCell { Border = 0 });
                }

                table.AddCell(new PdfPCell(text)
                {
                    Border = 0,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                });

                return new PdfPCell(table)
                {
                    Border = 0,
                    PaddingTop = 0,
                    PaddingBottom = 2,
                    PaddingLeft = 0
                };
            }

            var returnPhrase = new Paragraph();
            returnPhrase.SetLeading(0, 1.5f);
            returnPhrase.Add(new Chunk(T("Plugins.Misc.InvoicePDF.ReturnAddress"), labelFont));
            returnPhrase.Add(Chunk.Newline);
            returnPhrase.Add(new Chunk(ReturnAddress ?? "", normalFont));

            bottomSection.AddCell(BuildInfoCell(ReturnLogoData, returnPhrase, 5));
            bottomSection.AddCell(BuildInfoCell(OrderLogoData, new Phrase($"Order/Invoice# {OrderNumberText}", normalFont), 2));
            bottomSection.AddCell(BuildInfoCell(OrderDateLogoData, new Phrase($"Date: {OrderDateUser}", normalFont), 2));

            mainTable.AddCell(new PdfPCell(bottomSection)
            {
                Border = Rectangle.BOTTOM_BORDER,
                BorderColor = new BaseColor(6, 47, 79),
                BorderWidth = 1,
                PaddingLeft = 20,
                PaddingRight = 15,
                PaddingBottom = 0,
                BackgroundColor = new BaseColor(245, 245, 245),
            });

            #endregion

            #region Address Part

            mainTable.AddCell(new PdfPCell(CreateAdressesInfo())
            {
                Border = Rectangle.NO_BORDER,
                Padding = 0
            });

            #endregion

            return mainTable;
        }

        protected virtual PdfGrid CreateFooter(FooterData footerData)
        {
            var footerTable = PdfDocumentHelper.BuildPdfGrid(3, DocumentRunDirection);
            footerTable.WidthPercentage = 100;
            footerTable.SetWidths(new float[] { 33, 34, 33 });

            var bg = new BaseColor(245, 245, 245);
            var border = new BaseColor(6, 47, 79);
            var font = PdfDocumentHelper.GetFont(FontFactory.HELVETICA, 9);

            // Equal padding for all cells
            float cellPaddingTop = 12;
            float cellPaddingBottom = 12;
            float minCellHeight = 55;

            PdfPCell CreateCell()
            {
                return new PdfPCell
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderColorTop = border,
                    BorderWidthTop = 1,
                    BackgroundColor = bg,
                    PaddingTop = cellPaddingTop,
                    PaddingBottom = cellPaddingBottom,
                    PaddingLeft = 10,
                    PaddingRight = 10,
                    MinimumHeight = minCellHeight,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
            }

            // LEFT SECTION: Phone icon + numbers
            var leftTable = new PdfPTable(2);
            leftTable.WidthPercentage = 100;
            leftTable.SetWidths(new float[] { 20, 80 });

            var phoneImg = PdfImageHelper.GetITextSharpImageFromByteArray(PhoneLogoData);
            phoneImg.ScaleToFit(19f, 19f);

            var phoneIconCell = new PdfPCell(phoneImg, false)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingRight = 5,
                PaddingLeft = 5,
                PaddingTop = 5,
                PaddingBottom = 5
            };
            leftTable.AddCell(phoneIconCell);

            // Phone text with reduced spacing
            var phoneChunk1 = new Chunk("507-516-7113", font);
            phoneChunk1.SetAction(new PdfAction("tel:5075167113"));

            var phoneChunk2 = new Chunk("833-544-4872", font);
            phoneChunk2.SetAction(new PdfAction("tel:8335444872"));

            var phoneText = new Paragraph();
            phoneText.Font = font;
            phoneText.Font.Size = 12;
            phoneText.SetLeading(0, 1.4f);
            phoneText.Add(phoneChunk1);
            phoneText.Add(Chunk.Newline);
            phoneText.Add(phoneChunk2);

            var phoneTextCell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingRight = 0,
                PaddingLeft = 0,
                PaddingTop = 0,
                PaddingBottom = 0
            };
            phoneTextCell.AddElement(phoneText);
            leftTable.AddCell(phoneTextCell);

            var leftCell = CreateCell();
            leftCell.AddElement(leftTable);
            footerTable.AddCell(leftCell);

            // CENTER SECTION: Page number badge
            var pageCell = CreateCell();
            pageCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pageCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            pageCell.PaddingRight = 5;
            pageCell.PaddingLeft = 5;
            pageCell.PaddingTop = 5;
            pageCell.PaddingBottom = 5;
            pageCell.CellEvent = new PageNumberBadgeCellEvent(footerData.CurrentPageNumber);
            footerTable.AddCell(pageCell);

            // RIGHT SECTION: Mail icon + email/website
            var rightTable = new PdfPTable(2);
            rightTable.WidthPercentage = 100;
            rightTable.SetWidths(new float[] { 20, 80 });

            var mailImg = PdfImageHelper.GetITextSharpImageFromByteArray(MailLogoData);
            mailImg.ScaleToFit(19f, 19f);

            var mailIconCell = new PdfPCell(mailImg, false)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingRight = 5,
                PaddingLeft = 5,
                PaddingTop = 5,
                PaddingBottom = 5
            };
            rightTable.AddCell(mailIconCell);

            // Mail text with reduced spacing
            var emailChunk = new Chunk("info@larsenlights.com", font);
            emailChunk.SetAction(new PdfAction("mailto:info@larsenlights.com"));

            var websiteChunk = new Chunk("www.larsenlights.com", font);
            websiteChunk.SetAction(new PdfAction(StoreUrl));

            var mailText = new Paragraph();
            mailText.Font = font;
            mailText.Font.Size = 12;
            mailText.SetLeading(0, 1.4f);
            mailText.Add(emailChunk);
            mailText.Add(Chunk.Newline);
            mailText.Add(websiteChunk);

            var mailTextCell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingRight = 0,
                PaddingLeft = 0,
                PaddingTop = 0,
                PaddingBottom = 0
            };
            mailTextCell.AddElement(mailText);
            rightTable.AddCell(mailTextCell);

            var rightCell = CreateCell();
            rightCell.AddElement(rightTable);
            footerTable.AddCell(rightCell);

            return footerTable;
        }

        protected virtual PdfGrid CreateSummary()
        {
            // Wrapper table to handle the stamp overlay
            var wrapperTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);
            wrapperTable.WidthPercentage = 100;

            // Main summary table
            var summaryData = PdfDocumentHelper.BuildPdfGrid(numColumns: 2, DocumentRunDirection);
            summaryData.WidthPercentage = 100;
            summaryData.SetWidths(new float[] { 65, 35 });

            var normalFont = PdfDocumentHelper.GetFont(FontFactory.HELVETICA, 11, DocumentFontStyle.Normal);
            var boldFont = PdfDocumentHelper.GetFont(FontFactory.HELVETICA, 12, DocumentFontStyle.Bold);

            // Helper method to add a row
            void AddSummaryRow(string label, string value, Font labelFont = null, Font valueFont = null, bool isTotal = false)
            {
                labelFont ??= normalFont;
                valueFont ??= normalFont;

                var labelCell = new PdfPCell(new Phrase(label, labelFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingTop = 6,
                    PaddingBottom = isTotal ? 0 : 6,
                    PaddingLeft = 10,
                    PaddingRight = 3
                };

                var valueCell = new PdfPCell(new Phrase(value, valueFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    PaddingTop = 6,
                    PaddingBottom = isTotal ? 0 : 6,
                    PaddingLeft = 10,
                    PaddingRight = 3
                };

                // light separator for normal rows
                if (!isTotal)
                {
                    labelCell.Border = Rectangle.BOTTOM_BORDER;
                    labelCell.BorderColorBottom = new BaseColor(220, 220, 220);
                    labelCell.BorderWidthBottom = 0.8f;

                    valueCell.Border = Rectangle.BOTTOM_BORDER;
                    valueCell.BorderColorBottom = new BaseColor(220, 220, 220);
                    valueCell.BorderWidthBottom = 0.8f;
                }

                // Add borders for Order total row
                if (isTotal)
                {
                    labelCell.BorderWidthTop = 1;
                    labelCell.BorderColorTop = new BaseColor(6, 47, 79);
                    labelCell.BorderWidthBottom = 1;
                    labelCell.BorderColorBottom = new BaseColor(6, 47, 79);
                    labelCell.PaddingTop = 5;
                    labelCell.PaddingBottom = 0;
                    labelCell.BackgroundColor = new BaseColor(245, 245, 245);

                    valueCell.BorderWidthTop = 1;
                    valueCell.BorderColorTop = new BaseColor(6, 47, 79);
                    valueCell.BorderWidthBottom = 1;
                    valueCell.BorderColorBottom = new BaseColor(6, 47, 79);
                    valueCell.PaddingTop = 5;
                    valueCell.PaddingBottom = 0;
                    valueCell.BackgroundColor = new BaseColor(245, 245, 245);
                }

                summaryData.AddCell(labelCell);
                summaryData.AddCell(valueCell);
            }

            // sub total
            if (!string.IsNullOrEmpty(Totals.SubTotal))
                AddSummaryRow(T("Pdf.SubTotal"), Totals.SubTotal);

            // discount
            if (!string.IsNullOrEmpty(Totals.Discount))
                AddSummaryRow(T("Pdf.Discount"), Totals.Discount);

            // shipping
            if (!string.IsNullOrEmpty(Totals.Shipping))
                AddSummaryRow(T("Pdf.Shipping"), Totals.Shipping);

            // payment method additional fee
            if (!string.IsNullOrEmpty(Totals.PaymentMethodAdditionalFee))
                AddSummaryRow(T("Pdf.PaymentMethodAdditionalFee"), Totals.PaymentMethodAdditionalFee);

            // tax
            if (!string.IsNullOrEmpty(Totals.Tax))
                AddSummaryRow(T("Pdf.Tax"), Totals.Tax);

            // tax rates
            foreach (var rate in Totals.TaxRates)
                AddSummaryRow("", rate);

            // gift cards
            foreach (var card in Totals.GiftCards)
                AddSummaryRow("", card);

            // reward points
            if (!string.IsNullOrEmpty(Totals.RewardPoints))
                AddSummaryRow(T("Pdf.RewardPoints"), Totals.RewardPoints);

            // order total
            if (!string.IsNullOrEmpty(Totals.OrderTotal))
            {
                var orderTotalValue = Totals.OrderTotal;
                if (orderTotalValue.Contains("Order total:"))
                    orderTotalValue = orderTotalValue.Substring(orderTotalValue.IndexOf("Order total:") + "Order total:".Length).Trim();

                AddSummaryRow(T("Pdf.OrderTotal"), orderTotalValue, boldFont, boldFont, isTotal: true);
            }

            // Add the summary data to wrapper
            var summaryCell = new PdfPCell(summaryData)
            {
                Border = Rectangle.NO_BORDER,
                PaddingTop = 0,
                PaddingBottom = 0,
                PaddingLeft = 10,
                PaddingRight = 10
            };

            // paid logo
            if (PaymentStatus == (int)Core.Domain.Payments.PaymentStatus.Paid && PaidLogoData != null)
                summaryCell.CellEvent = new StampCellEvent(PaidLogoData);

            // due logo
            if (PaymentStatus == (int)Core.Domain.Payments.PaymentStatus.Pending && DueLogoData != null)
                summaryCell.CellEvent = new StampCellEvent(DueLogoData);

            wrapperTable.AddCell(summaryCell);

            return wrapperTable;
        }

        protected virtual PdfGrid CreateCheckoutAttributes()
        {
            var attributesData = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);

            attributesData.AddCell(BuildPdfPCell(CheckoutAttributes));

            return attributesData;
        }

        protected virtual PdfGrid CreateOrderNotes()
        {
            var notesTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 2, DocumentRunDirection);

            if (OrderNotes?.Any() != true)
                return notesTable;

            notesTable.SetWidths([2, 5]);

            var fontBold = PdfDocumentHelper.GetFont(Font, Font.Size, DocumentFontStyle.Bold);
            var label = LabelField<InvoiceDocument, List<(string, string)>>(invoice => invoice.OrderNotes, fontBold, Language);

            notesTable.AddCell(
                new PdfPCell(new Phrase(label))
                {
                    Border = 0,
                    Colspan = 2,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingBottom = 5,
                    RunDirection = DocumentRunDirection,
                });

            foreach (var (date, note) in OrderNotes)
            {
                notesTable.AddCell(BuildPdfPCell(Language.Rtl ? date.FixWeakCharacters() : date));
                notesTable.AddCell(BuildPdfPCell(note));
            }

            return notesTable;
        }

        protected string T(string resourceKey)
        {
            return EngineContext.Current.Resolve<ILocalizationService>().GetResourceAsync(resourceKey).Result;
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
                .DocumentPreferences(doc =>
                {
                    doc.DocumentMargins(new DocumentMargins
                    {
                        Top = 0,
                        Bottom = 80,
                        Left = 0,
                        Right = 0
                    });
                })
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
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.Name, width: 8, printProductAttributes: true));
                    if (ShowSkuInProductList)
                        columns.AddColumn(column => ConfigureProductColumn(column, p => p.Sku, width: 3));
                    if (ShowVendorInProductList)
                        columns.AddColumn(column => ConfigureProductColumn(column, p => p.VendorName, width: 3));
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.Price, width: 3));
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.Quantity, width: 2));
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.Total, width: 3));

                })
                .MainTableEvents(events =>
                {
                    events.MainTableCreated(events =>
                    {
                        //add to body, since adding hyperlinks to document header is not allowed
                        events.PdfDoc.Add(CreateInvoiceHeader());
                        events.Table.WidthPercentage = 94; 
                        events.Table.HorizontalAlignment = Element.ALIGN_CENTER;
                        events.Table.DefaultCell.Border = Rectangle.NO_BORDER;

                    });

                    events.MainTableAdded(events =>
                    {
                        var summaryTable = PdfDocumentHelper.BuildPdfGrid(2, DocumentRunDirection);
                        summaryTable.WidthPercentage = 100;
                        summaryTable.DefaultCell.Border = Rectangle.NO_BORDER;

                        summaryTable.SetWidths(new float[] { 60, 40 });
                        // LEFT: Checkout attributes
                        var leftCell = new PdfPCell(CreateCheckoutAttributes())
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            VerticalAlignment = Element.ALIGN_TOP,
                            PaddingLeft = 20,
                            PaddingRight = 20
                        };
                        summaryTable.AddCell(leftCell);

                        // RIGHT: Summary
                        var rightCell = new PdfPCell(CreateSummary())
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            VerticalAlignment = Element.ALIGN_TOP,
                            PaddingLeft = 12,
                            PaddingRight = 12
                        };
                        summaryTable.AddCell(rightCell);

                        events.PdfDoc.Add(summaryTable);
                        events.PdfDoc.Add(CreateOrderNotes());
                    });
                })
                .Generate(builder => builder.AsPdfStream(pdfStreamOutput, closeStream: false));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the logo binary
        /// </summary>
        public byte[] LogoData { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        [DisplayName("Pdf.OrderDate")]
        public required string OrderDateUser { get; init; }

        /// <summary>
        /// Gets or sets the order number
        /// </summary>
        [DisplayName("Pdf.Order")]
        public required string OrderNumberText { get; init; }

        /// <summary>
        /// Gets or sets store location
        /// </summary>
        public string StoreUrl { get; init; }

        /// <summary>
        /// Gets or sets the billing address
        /// </summary>
        [DisplayName("Pdf.BillingInformation")]
        public required AddressItem BillingAddress { get; init; }

        /// <summary>
        /// Gets or sets the shipping address
        /// </summary>
        [DisplayName("Pdf.ShippingInformation")]
        public AddressItem ShippingAddress { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether to display product SKU in the invoice document
        /// </summary>
        public bool ShowSkuInProductList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display vendor name in the invoice document
        /// </summary>
        public bool ShowVendorInProductList { get; set; }

        /// <summary>
        /// Gets or sets the checkout attribute description
        /// </summary>
        public string CheckoutAttributes { get; set; }

        /// <summary>
        /// Gets or sets order totals
        /// </summary>
        public InvoiceTotals Totals { get; set; } = new();

        /// <summary>
        /// Gets or sets order notes
        /// </summary>
        [DisplayName("Pdf.OrderNotes")]
        public List<(string, string)> OrderNotes { get; set; }

        /// <summary>
        /// Gets or sets the text that will appear at the bottom of invoice (column 1)
        /// </summary>
        public List<string> FooterTextColumn1 { get; set; } = new();

        /// <summary>
        /// Gets or sets the text that will appear at the bottom of invoice (column 2)
        /// </summary>
        public List<string> FooterTextColumn2 { get; set; } = new();

        public int PaymentStatus { get; set; }

        [DisplayName("Plugins.Misc.InvoicePDF.TrackingNumber")]
        public List<(string, string)> TrackingNumber { get; set; }

        [DisplayName("Plugins.Misc.InvoicePDF.ReturnAddress")]
        public string ReturnAddress { get; set; }

        public byte[] ReturnLogoData { get; set; }

        public byte[] OrderLogoData { get; set; }

        public byte[] OrderDateLogoData { get; set; }

        public byte[] DueLogoData { get; set; }

        public byte[] PaidLogoData { get; set; }

        public byte[] PhoneLogoData { get; set; }

        public byte[] MailLogoData { get; set; }

        public byte[] FoldLogoData { get; set; }

        #endregion
    }
}
