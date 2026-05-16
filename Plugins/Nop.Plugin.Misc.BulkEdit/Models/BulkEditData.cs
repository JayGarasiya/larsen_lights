using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.BulkEdit.Models
{
    public class BulkEditData
    {
        private bool _updated;
        private bool _created;
        private readonly int _defaultTaxCategoryId;
        private readonly int _vendorId;

        public BulkEditData(int defaultTaxCategoryId, int vendorId)
        {
            _defaultTaxCategoryId = defaultTaxCategoryId;
            _vendorId = vendorId;
        }

        public bool IsSelected { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsPublished { get; set; }
        public decimal ProductCost { get; set; }
        public Product Product { get; set; }

        public bool NeedToUpdate(bool selected)
        {
            if (selected && !IsSelected)
                return false;

            if (Product == null)
                return false;

            if (_updated)
                return true;

            return IsStringChanged(Product.Name, Name) ||
                   IsStringChanged(Product.Sku, Sku) ||
                   Product.Price != Price ||
                   Product.OldPrice != OldPrice ||
                   Product.StockQuantity != Quantity ||
                   Product.ProductCost != ProductCost ||
                   Product.Published != IsPublished;
        }

        public bool NeedToCreate(bool selected)
        {
            if (selected && !IsSelected)
                return false;

            if (Product != null)
                return false;

            return true;
        }

        public Product UpdateProduct(bool selected)
        {
            if (!NeedToUpdate(selected) || _updated)
                return Product;

            Product.Name = Name;
            Product.Sku = Sku;
            Product.Price = Price;
            Product.OldPrice = OldPrice;
            Product.StockQuantity = Quantity;
            Product.Published = IsPublished;
            Product.ProductCost = ProductCost;

            _updated = true;

            return Product;
        }

        public Product CreateProduct(bool selected)
        {
            if (!NeedToCreate(selected) || _created)
                return Product;

            Product = new Product
            {
                Name = Name,
                Sku = Sku,
                Price = Price,
                OldPrice = OldPrice,
                StockQuantity = Quantity,
                Published = IsPublished,
                ProductCost = ProductCost,

                MaximumCustomerEnteredPrice = 1000,
                MaxNumberOfDownloads = 10,
                RecurringCycleLength = 100,
                RecurringTotalCycles = 10,
                RentalPriceLength = 1,
                NotifyAdminForQuantityBelow = 1,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                TaxCategoryId = _defaultTaxCategoryId,
                UnlimitedDownloads = true,
                IsShipEnabled = true,
                AllowCustomerReviews = true,
                VisibleIndividually = true,
                ProductType = ProductType.SimpleProduct,
                VendorId = _vendorId
            };

            _created = true;

            return Product;
        }

        private bool IsStringChanged(string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(oldValue))
                return !string.IsNullOrEmpty(newValue);

            return !oldValue.Equals(newValue);
        }
    }
}
