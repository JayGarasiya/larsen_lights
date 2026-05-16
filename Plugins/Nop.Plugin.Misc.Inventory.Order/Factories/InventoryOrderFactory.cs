using ClosedXML.Excel;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.Inventory.Order.Domain;
using Nop.Plugin.Misc.Inventory.Order.Models;
using Nop.Plugin.Misc.Inventory.Order.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Common.Pdf;
using Nop.Services.Configuration;
using Nop.Services.ExportImport.Help;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Models.Extensions;
using PdfRpt.Core.Contracts;
using System.Globalization;
using System.IO.Compression;

namespace Nop.Plugin.Misc.Inventory.Order.Factories
{
    /// <summary>
    /// Represents the inventory order factory
    /// </summary>
    public partial class InventoryOrderFactory : IInventoryOrderFactory
    {
        #region Fields
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly INopFileProvider _fileProvider;
        protected readonly ILocalizationService _localizationService;
        protected readonly IStoreContext _storeContext;
        protected readonly IWorkContext _workContext;
        protected readonly IPictureService _pictureService;
        protected readonly IRepository<PoOrder> _poorderRepository;
        protected readonly IRepository<PoOrderItem> _poorderitemRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<Manufacturer> _manufacturerRepository;
        protected readonly CatalogSettings _catalogSettings;
        protected readonly IPoOrderService _poOrderService;
        protected readonly ISettingService _settingService;
        protected readonly InventoryOrderSettings _inventoryOrderSettings;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IThumbService _thumbService;
        protected readonly IPoOrderService _poorderService;
        protected readonly IProductService _productService;
        protected readonly IManufacturerService _manufacturerService;
        #endregion

        #region Ctor
        public InventoryOrderFactory(IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            INopFileProvider fileProvider,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IPictureService pictureService,
            IRepository<PoOrder> poorderRepository,
            IRepository<PoOrderItem> poorderitemRepository,
            IRepository<Product> productRepository,
            IRepository<Manufacturer> manufacturerRepository,
            CatalogSettings catalogSettings,
            IPoOrderService poOrderService,
            ISettingService settingService,
            InventoryOrderSettings inventoryOrderSettings,
            IPriceFormatter priceFormatter,
            IThumbService thumbService,
            IPoOrderService poorderService,
            IProductService productService,
            IManufacturerService manufacturerService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _fileProvider = fileProvider;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
            _pictureService = pictureService;
            _poorderRepository = poorderRepository;
            _poorderitemRepository = poorderitemRepository;
            _productRepository = productRepository;
            _manufacturerRepository = manufacturerRepository;
            _catalogSettings = catalogSettings;
            _poOrderService = poOrderService;
            _settingService = settingService;
            _inventoryOrderSettings = inventoryOrderSettings;
            _priceFormatter = priceFormatter;
            _thumbService = thumbService;
            _poorderService = poorderService;
            _productService = productService;
            _manufacturerService = manufacturerService;
        }
        #endregion

        #region Utilities

        #region Excel Class
        public class PoOrderItemExcel
        {
            public int Id { get; set; }
            public bool HasReceived { get; set; }
            public string PONumber { get; set; }
            public string AdminComment { get; set; }
            public string PercentAdjusted { get; set; }
        }

        public class PoOrderItems
        {
            public string PONumber { get; set; }
            public string Manufacturer { get; set; }
            public string Productname { get; set; }
            public decimal ProductCost { get; set; }
            public string Sku { get; set; }
            public int Qty { get; set; }
            public static int ProducAttributeCellOffset { get; } = 2;
        }

        public class PpOrderExcel
        {
            public int ManufactureId { get; set; }
            public string Manufacture { get; set; }
            public string Sku { get; set; }
            public decimal ProductCost { get; set; }
            public int QTY { get; set; }
        }
        #endregion

        /// <summary>
        /// Resolve font for PDF document
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="settings">PDF settings</param>
        /// <returns>A font object</returns>
        protected virtual iTextSharp.text.Font ResolvePdfFont(Language language, PdfSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var fontName = language?.Rtl == true
                ? !string.IsNullOrEmpty(settings.RtlFontName) ? settings.RtlFontName : NopCommonDefaults.PdfRtlFontName
                : !string.IsNullOrEmpty(settings.LtrFontName) ? settings.LtrFontName : NopCommonDefaults.PdfLtrFontName;

            var fontSize = settings.BaseFontSize >= 0 ? settings.BaseFontSize : 10;

            return PdfDocumentHelper.GetFont(fontName, fontSize);
        }

        /// <summary>
        /// Get po order item
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the get po order item
        /// </returns>
        private PropertyManager<PoOrderItems> GetPoOrderItem()
        {
            var properties = new[]
            {
                new PropertyByName<PoOrderItems>("Manufacture", (p, l) => p.Manufacturer),
                new PropertyByName<PoOrderItems>("ProductName", (p, l) => p.Productname),
                new PropertyByName<PoOrderItems>("Sku", (p, l) => p.Sku),
                new PropertyByName<PoOrderItems>("Product Cost", (p, l) => p.ProductCost),
                new PropertyByName<PoOrderItems>("Qty", (p, l) => p.Qty),
            };
            return new PropertyManager<PoOrderItems>(properties, _catalogSettings);
        }

        /// <summary>
        /// Export pO to xlsx with child async
        /// </summary>
        /// <param name="properties">properties</param>
        /// <param name="itemsToExport">itemsToExport</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the export po to xlsx with child
        /// </returns>
        private async Task<byte[]> ExportPOToXlsxWithChildAsync(PropertyByName<PoOrderItemExcel>[] properties, IEnumerable<PoOrderItemExcel> itemsToExport)
        {
            var productAttributeManager = GetPoOrderItem();
            await using var stream = new MemoryStream();
            using (var xlPackage = new XLWorkbook())
            {
                var worksheet = xlPackage.Worksheets.Add(typeof(Core.Domain.Catalog.Product).Name);
                var fpWorksheet = xlPackage.Worksheets.Add("DataForProductsFilters1");
                fpWorksheet.Visibility = XLWorksheetVisibility.VeryHidden;

                var fbaWorksheet = xlPackage.Worksheets.Add("DataForProductAttrsFilters1");
                fbaWorksheet.Visibility = XLWorksheetVisibility.VeryHidden;
                var manager = new PropertyManager<PoOrderItemExcel>(properties, _catalogSettings);
                manager.WriteDefaultCaption(worksheet);
                var row = 2;
                foreach (var item in itemsToExport)
                {
                    manager.CurrentObject = item;
                    await manager.WriteDefaultToXlsxAsync(worksheet, row++, fWorksheet: fpWorksheet);

                    if (_catalogSettings.ExportImportProductAttributes)
                        row = await ExportPoorderitemsAsync(productAttributeManager, worksheet, row, fbaWorksheet, item.PONumber);
                }
                xlPackage.SaveAs(stream);
            }
            return stream.ToArray();
        }

        /// <summary>
        /// Export po order items async
        /// </summary>
        /// <param name="attributeManager">attributeManager</param>
        /// <param name="worksheet">worksheet</param>
        /// <param name="row">row</param>
        /// <param name="faWorksheet">faWorksheet</param>
        /// <param name="poNumber">poNumber</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the export po order items
        /// </returns>
        private async Task<int> ExportPoorderitemsAsync(PropertyManager<PoOrderItems> attributeManager, IXLWorksheet worksheet, int row, IXLWorksheet faWorksheet, string poNumber)
        {
            var poOrderItem = await (from po in _poorderRepository.Table
                                     join poi in _poorderitemRepository.Table
                                     on po.Id equals poi.PoOrderId
                                     join pro in _productRepository.Table
                                     on poi.ProductId equals pro.Id
                                     join manu in _manufacturerRepository.Table
                                     on poi.ManufacturerId equals manu.Id
                                     where po.PONumber.ToLower() == poNumber.ToLower()
                                     select new PoOrderItems
                                     {
                                         Manufacturer = manu.Name,
                                         Productname = pro.Name,
                                         Sku = pro.Sku,
                                         ProductCost = pro.ProductCost,
                                         Qty = poi.OrderedQty
                                     }).ToListAsync();

            if (!poOrderItem.Any())
                return row;

            attributeManager.WriteDefaultCaption(worksheet, row, PoOrderItems.ProducAttributeCellOffset);
            worksheet.Row(row).OutlineLevel = 1;
            worksheet.Row(row).Collapse();

            foreach (var exportProductAttribute in poOrderItem)
            {
                row++;
                attributeManager.CurrentObject = exportProductAttribute;
                await attributeManager.WriteDefaultToXlsxAsync(worksheet, row, PoOrderItems.ProducAttributeCellOffset, faWorksheet);
                worksheet.Row(row).OutlineLevel = 1;
                worksheet.Row(row).Collapse();
            }
            return row + 1;
        }
        #endregion

        #region Methods

        #region Po Order
        /// <summary>
        /// Prepare the manage inventory search model
        /// </summary>
        /// <param name="searchModel">Manage Inventory Search Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manage inventory search model
        /// </returns>
        public async Task<ManageInventoryListModel> PrepareManageInventorySearchModel(ManageInventorySearchModel searchModel)
        {
            var startDateValue = !searchModel.StartDate.HasValue ? DateTime.UtcNow.AddYears(-200)
                 : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var endDateValue = !searchModel.EndDate.HasValue ? DateTime.UtcNow.AddYears(200)
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            var orderInventoryReports = await _poOrderService.PrepareMangeOrderReportModel(startDateValue: startDateValue, endDateValue: endDateValue, searchManufacturerId: searchModel.SearchManufacturerId, searchCategoryId: searchModel.SearchCategoryId,
               searchPercentage: searchModel.SearchPercentage, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new ManageInventoryListModel().PrepareToGridAsync(searchModel, orderInventoryReports, () =>
            {
                return orderInventoryReports.SelectAwait(async orderInventoryReport =>
                {
                    var orderQty = orderInventoryReport.OrderedQty.Split(',');
                    var orderQtyInt1 = 0;
                    var orderQtyInt2 = 0;
                    var orderQtyInt3 = 0;
                    var orderQtyInt4 = 0;
                    int.TryParse(orderQty[0] ?? string.Empty, out orderQtyInt1);
                    int.TryParse(orderQty.Count() > 1 ? orderQty[1] : string.Empty, out orderQtyInt2);
                    int.TryParse(orderQty.Count() > 2 ? orderQty[2] : string.Empty, out orderQtyInt3);
                    int.TryParse(orderQty.Count() > 3 ? orderQty[3] : string.Empty, out orderQtyInt4);
                    var totalQty = (orderInventoryReport.Needed - (orderQtyInt1 + orderQtyInt2 + orderQtyInt3 + orderQtyInt4)) - orderInventoryReport.InStock;
                    var boxVolume = Math.Round(orderInventoryReport.BoxVolume, 4) * Math.Ceiling(totalQty / orderInventoryReport.TotalCartoon);

                    var inventoryModel = new ManageInventoryModel
                    {
                        Id = orderInventoryReport.ProductId,
                        ProductId = orderInventoryReport.ProductId,
                        ManufacturerId = orderInventoryReport.ManufactureId,
                        Manufacture = orderInventoryReport.Manufacture,
                        Name = orderInventoryReport.Name,
                        InStock = orderInventoryReport.InStock,
                        SKU = orderInventoryReport.Sku,
                        TimeFrameQty = orderInventoryReport.QTY,
                        IncOrDesQty = orderInventoryReport.Needed,
                        ManufactureProductIds = $"{orderInventoryReport.ManufactureId},{orderInventoryReport.ProductId},{totalQty},{(boxVolume < 0 ? "0" : boxVolume)}",
                        OrderedQty1 = orderQtyInt1,
                        OrderedQty2 = orderQtyInt2,
                        OrderedQty3 = orderQtyInt3,
                        OrderedQty4 = orderQtyInt4,
                        TotalQty = totalQty,
                        TotalCartoon = Math.Ceiling(totalQty / orderInventoryReport.TotalCartoon),
                        BoxVolume = totalQty < 0 ? 0 : boxVolume,
                        TotalProductQtyCartoon = Convert.ToInt32(orderInventoryReport.TotalCartoon),
                        TotalBoxVolume = Math.Round(orderInventoryReport.BoxVolume, 4),
                        IsNegative = totalQty < 0,
                        IsBoxVolume = totalQty < 0,
                        ProductCost = orderInventoryReport.ProductCost,
                        Published = orderInventoryReport.Published
                    };

                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(orderInventoryReport.ProductId, 1)).FirstOrDefault();
                    (inventoryModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);

                    return inventoryModel;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare the manage inventory list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manage inventory list model
        /// </returns>
        public async Task<ManageInventorySearchModel> PrepareManageInventoryListModel()
        {
            var searchModel = new ManageInventorySearchModel();

            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            searchModel.AvailableManufacturers = searchModel.AvailableManufacturers.Where(m => (_inventoryOrderSettings.AllowedManufacturers != null && _inventoryOrderSettings.AllowedManufacturers.Contains(int.Parse(m.Value))) || int.Parse(m.Value) == 0).ToList();

            searchModel.SetGridPageSize();

            return searchModel;
        }
        #endregion

        #region PoOrder Item
        /// <summary>
        /// Prepare the po order search model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the po order search model
        /// </returns>
        public Task<PoOrderSearchModel> PreparePoOrderSearchModel(PoOrderSearchModel searchModel)
        {
            searchModel.PoOrderItemSearchModel.SetGridPageSize();
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare the po order list model
        /// </summary>
        /// <param name="searchModel">Po Order Item Search Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the po order list model
        /// </returns>
        public async Task<PoOrderListModel> PreparePoOrderListModel(PoOrderSearchModel searchModel)
        {
            var poOrders = await _poorderService.GetAllPoOrder(poNumber: searchModel.SearchPoNumber,
                adminComment: searchModel.SearchAdminComment,
                startDate: searchModel.StartDate,
                endDate: searchModel.EndDate,
                searchModel.Page - 1, searchModel.PageSize, true);

            var gridmodel = new PoOrderListModel().PrepareToGrid(searchModel, poOrders, () =>
            {
                return poOrders.Select(x =>
                {
                    var poOrderModel = new PoOrderModel();
                    poOrderModel.Id = x.Id;
                    poOrderModel.PONumber = x.PONumber;
                    poOrderModel.Comment = x.Comment;
                    poOrderModel.HasReceived = x.HasReceived;
                    poOrderModel.PercentAdjusted = x.PercentAdjusted;
                    poOrderModel.CreatedOnUTC = x.CreatedOnUTC;
                    poOrderModel.ReceivedOnUTC = x.ReceivedOnUTC;
                    poOrderModel.TotalVolume = x.TotalVolume;
                    poOrderModel.AvailableDateOnUTC = x.AvailableDateOnUTC;
                    return poOrderModel;
                });
            });

            return gridmodel;
        }

        /// <summary>
        /// Prepare the po order item by po number
        /// </summary>
        /// <param name="searchModel">po order item search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the po order item list model
        /// </returns>
        public async Task<PoOrderItemListModel> PreparePoOrderItemByPoNumber(PoOrderItemSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            searchModel.Length = int.MaxValue;

            var poOrderItems = (await _poorderService.GetPoOrderItemsByPoOrderAsync(searchModel.PoOrderId, searchModel.Page - 1, searchModel.PageSize, true)).ToPagedList(searchModel);

            var gridmodel = await new PoOrderItemListModel().PrepareToGridAsync(searchModel, poOrderItems, () =>
            {
                return poOrderItems.SelectAwait(async x =>
                {
                    var product = await _productService.GetProductByIdAsync(x.ProductId);
                    var manufacture = await _manufacturerService.GetManufacturerByIdAsync(x.ManufacturerId);
                    var poOrderItemModel = new PoOrderItemModel
                    {
                        Id = x.Id,
                        ManufacturerName = manufacture?.Name ?? string.Empty,
                        ProductName = product?.Name ?? string.Empty,
                        Sku = product?.Sku ?? string.Empty,
                        ProductCost = product?.ProductCost ?? decimal.Zero,
                        OrderedQty = x.OrderedQty,
                        Published = product?.Published ?? false,
                    };

                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(x.ProductId, 1)).FirstOrDefault();
                    (poOrderItemModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);

                    return poOrderItemModel;
                });
            });

            return gridmodel;
        }
        #endregion

        #region PoOrderItem PDF
        /// <summary>
        /// Print po orders to pdf async
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="poOrders">PoOrder</param>
        /// <param name="language">Language</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the print po orders to pdf
        /// </returns>
        public virtual async Task PrintPoOrdersToPdfAsync(Stream stream, IList<PoOrder> poOrders, Language language = null)
        {
            ArgumentNullException.ThrowIfNull(stream);

            ArgumentNullException.ThrowIfNull(poOrders);

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true);

            foreach (var poOrder in poOrders)
            {
                var entryName = string.Format("{0}", poOrder.PONumber);

                await using var fileStreamInZip = archive.CreateEntry($"{entryName}.pdf").Open();
                await using var pdfStream = new MemoryStream();
                await PrintPoOrderToPdfAsync(pdfStream, poOrder, language, currentStore);
                pdfStream.Position = 0;
                await pdfStream.CopyToAsync(fileStreamInZip);
            }
        }

        /// <summary>
        /// Print po order To pdf async
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="poOrder">PoOrder</param>
        /// <param name="language">Language</param>
        /// <param name="store">Store</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the print po order to pdf
        /// </returns>
        public virtual async Task PrintPoOrderToPdfAsync(Stream stream, PoOrder poOrder, Language language = null, Store store = null)
        {
            var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(store.Id);

            byte[] logo = null;
            var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
            if (logoPicture != null)
            {
                var (pictureUrl, _) = await _pictureService.GetPictureUrlAsync(logoPicture, pdfSettingsByStore.ImageTargetSize, false);
                var logoFilePath = await _thumbService.GetThumbLocalPathAsync(pictureUrl);

                logo = await _pictureService.LoadPictureBinaryAsync(logoPicture);

                if (logoPicture.MimeType == MimeTypes.ImageSvg)
                {
                    await using var logoStream = new MemoryStream(logo);
                    logo = await _pictureService.ConvertSvgToPngAsync(logoStream);
                }
                else
                    logo = await _fileProvider.ReadAllBytesAsync(logoFilePath);
            }

            var date = await _dateTimeHelper.ConvertToUserTimeAsync(poOrder.CreatedOnUTC, DateTimeKind.Utc);

            var products = await (from poi in _poorderitemRepository.Table
                                  join pro in _productRepository.Table on poi.ProductId equals pro.Id
                                  join manu in _manufacturerRepository.Table on poi.ManufacturerId equals manu.Id
                                  where poi.PoOrderId == poOrder.Id
                                  select new
                                  {
                                      ManufacturerName = manu.Name,
                                      ProductName = pro.Name,
                                      SKU = pro.Sku,
                                      ProductCost = pro.ProductCost,
                                      QTY = poi.OrderedQty
                                  }).ToListAsync();

            var source = new PdfInvoiceDocument
            {
                StoreUrl = store.Url?.Trim('/'),
                Language = language,
                Font = ResolvePdfFont(language, pdfSettingsByStore),
                OrderDateUser = date.ToString("D", new CultureInfo(language.LanguageCulture)),
                LogoData = logo,
                CompanyAddress = string.Join(" ", new string[] { store.CompanyAddress, store.CompanyPhoneNumber }),
                OrderNumberText = poOrder.PONumber,
                OrderComment = poOrder.Comment,
                TotalVolume = poOrder.TotalVolume.ToString(),
                Products = await products.SelectAwait(async p => new PdfProductItems()
                {
                    ManufacturerName = p.ManufacturerName,
                    ProductName = p.ProductName,
                    SKU = p.SKU,
                    ProductCost = await _priceFormatter.FormatPriceAsync(p.ProductCost, true,
                        await _workContext.GetWorkingCurrencyAsync(), language.Id, false),
                    QTY = p.QTY.ToString()
                }).ToListAsync(),
                PageSize = pdfSettingsByStore.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4,
                ImageTargetSize = pdfSettingsByStore.ImageTargetSize,
                GetResourceAsync = async (string resourceKey, int languageId) => await _localizationService.GetResourceAsync(resourceKey, languageId)
            };

            await using var pdfStream = new MemoryStream();
            source.Generate(pdfStream);

            pdfStream.Position = 0;
            await pdfStream.CopyToAsync(stream);
        }
        #endregion

        #region PoOrderItem Excel
        /// <summary>
        /// Export po order item all to xlsx async
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the export po order item all to xlsx
        /// </returns>
        public virtual async Task<byte[]> ExportPoOrderItemAllToXlsxAsync()
        {
            var poOrderItems = await (from po in _poorderRepository.Table
                                     select new PoOrderItemExcel
                                     {
                                         Id = po.Id,
                                         HasReceived = po.HasReceived,
                                         PONumber = po.PONumber,
                                         AdminComment = po.Comment,
                                         PercentAdjusted = po.PercentAdjusted.ToString(),
                                     }).ToListAsync();

            var properties = new[]
            {
                new PropertyByName<PoOrderItemExcel>("Id", (p,l) => p.Id),
                new PropertyByName<PoOrderItemExcel>("Received", (p,l) => p.HasReceived),
                new PropertyByName<PoOrderItemExcel>("PoNumber", (p,l) => p.PONumber),
                new PropertyByName<PoOrderItemExcel>("Admin Comment", (p,l) => p.AdminComment),
                new PropertyByName<PoOrderItemExcel>("PercentAdjusted", (p,l) => p.PercentAdjusted + " %"),
            };

            if (!_catalogSettings.ExportImportProductAttributes && !_catalogSettings.ExportImportProductSpecificationAttributes)
                return await new PropertyManager<PoOrderItemExcel>(properties, _catalogSettings).ExportToXlsxAsync(poOrderItems);

            return await ExportPOToXlsxWithChildAsync(properties, poOrderItems);
        }

        /// <summary>
        /// Export po order item to xlsx async
        /// </summary>
        /// <param name="selectid">selectid</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the export po order item to xlsx
        /// </returns>
        public virtual async Task<byte[]> ExportPoOrderItemToXlsxAsync(IList<int> selectid)
        {
            var poOrderItems = await (from po in _poorderRepository.Table
                                     select new PoOrderItemExcel
                                     {
                                         Id = po.Id,
                                         HasReceived = po.HasReceived,
                                         PONumber = po.PONumber,
                                         AdminComment = po.Comment,
                                         PercentAdjusted = po.PercentAdjusted.ToString(),
                                     }).ToListAsync();

            poOrderItems = await poOrderItems.Where(x => selectid.Contains(x.Id)).ToListAsync();

            var properties = new[]
            {
                new PropertyByName<PoOrderItemExcel>("Id", (p,l) => p.Id),
                new PropertyByName<PoOrderItemExcel>("Received", (p,l) => p.HasReceived),
                new PropertyByName<PoOrderItemExcel>("PoNumber", (p,l) => p.PONumber),
                new PropertyByName<PoOrderItemExcel>("Admin Comment", (p,l) => p.AdminComment),
                new PropertyByName<PoOrderItemExcel>("PercentAdjusted", (p,l) => p.PercentAdjusted + " %"),
            };

            if (!_catalogSettings.ExportImportProductAttributes && !_catalogSettings.ExportImportProductSpecificationAttributes)
                return await new PropertyManager<PoOrderItemExcel>(properties, _catalogSettings).ExportToXlsxAsync(poOrderItems);

            return await ExportPOToXlsxWithChildAsync(properties, poOrderItems);
        }

        #endregion

        #region PoOrder Excel
        /// <summary>
        /// Export po order to xlsx
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the export po order to xlsx
        /// </returns>
        public virtual async Task<byte[]> ExportPoOrderToXlsx(ManageInventorySearchModel searchModel)
        {
            var ids = new List<int>();

            if (searchModel.SelectedIds != null)
            {
                ids = searchModel.SelectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();
            }

            var orderInventoryReports = (await PrepareManageInventorySearchModel(searchModel)).Data;

            if (ids.Count > 0)
                orderInventoryReports = orderInventoryReports.Where(x => ids.Contains(x.ProductId));

            var excelOrderReport = await orderInventoryReports.GroupBy(x => new { x.Manufacture, x.SKU, x.TotalQty, x.ProductCost }).Select(x =>
            new PpOrderExcel
            {
                Manufacture = x.Key.Manufacture,
                Sku = x.Key.SKU,
                ProductCost = x.Key.ProductCost,
                QTY = x.Key.TotalQty
            }).ToListAsync();

            var properties = new[]
            {
                new PropertyByName<PpOrderExcel>("Manufacturer", (p, l) => p.Manufacture),
                new PropertyByName<PpOrderExcel>("Sku", (p, l) => p.Sku),
                new PropertyByName<PpOrderExcel>("Product Cost", (p, l) => p.ProductCost),
                new PropertyByName<PpOrderExcel>("Total Qty", (p, l) => p.QTY),
            };
            if (!_catalogSettings.ExportImportProductAttributes && !_catalogSettings.ExportImportProductSpecificationAttributes)
                return await new PropertyManager<PpOrderExcel>(properties, _catalogSettings).ExportToXlsxAsync(excelOrderReport);

            return await new PropertyManager<PpOrderExcel>(properties, _catalogSettings).ExportToXlsxAsync(excelOrderReport);
        }
        #endregion

        #endregion
    }
}
