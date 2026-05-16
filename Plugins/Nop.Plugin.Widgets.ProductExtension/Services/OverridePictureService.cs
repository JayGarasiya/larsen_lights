using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Plugin.Widgets.ProductExtension.Services
{
    /// <summary>
    /// Picture service
    /// </summary>
    public class OverridePictureService : PictureService
    {
        #region Ctor
        public OverridePictureService(IDownloadService downloadService, 
            IHttpContextAccessor httpContextAccessor, 
            ILogger logger, 
            INopFileProvider fileProvider, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IRepository<Picture> pictureRepository, 
            IRepository<PictureBinary> pictureBinaryRepository, 
            IRepository<ProductPicture> productPictureRepository, 
            ISettingService settingService, 
            IThumbService thumbService, 
            IUrlRecordService urlRecordService, 
            IWebHelper webHelper, 
            MediaSettings mediaSettings) : base(
                downloadService, 
                httpContextAccessor, 
                logger, 
                fileProvider, 
                productAttributeParser, 
                productAttributeService, 
                pictureRepository, 
                pictureBinaryRepository, 
                productPictureRepository, 
                settingService, 
                thumbService, 
                urlRecordService, 
                webHelper, 
                mediaSettings)
        {
        }

        #endregion

        #region Method

        /// <summary>
        /// Get product picture (for shopping cart and order details pages)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes (in XML format)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the picture
        /// </returns>
        public override async Task<Picture> GetProductPictureAsync(Product product, string attributesXml)
        {
            ArgumentNullException.ThrowIfNull(product);

            //first, try to get product attribute combination picture
            var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
            if (combination != null)
            {
                var combinationPicture = (await _productAttributeService.GetProductAttributeCombinationPicturesAsync(combination.Id)).FirstOrDefault();
                if (await GetPictureByIdAsync(combinationPicture?.PictureId ?? 0) is Picture picture)
                    return picture;
            }

            //restric to show selected attribute picture out side product detail page
            if (string.IsNullOrEmpty(attributesXml))
            {
                //then, let's see whether we have attribute values with pictures
                var values = await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml);
                foreach (var attributeValue in values)
                {
                    var valuePictures = await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id);
                    var attributePicture = (await GetPicturesByIdsAsync(valuePictures.Select(vp => vp.PictureId).ToArray())).FirstOrDefault();

                    if (attributePicture != null)
                        return attributePicture;
                }
            }

            //now let's load the default product picture
            var productPicture = (await GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
            if (productPicture != null)
                return productPicture;

            //finally, let's check whether this product has some parent "grouped" product
            if (product.VisibleIndividually || product.ParentGroupedProductId <= 0)
                return null;

            var parentGroupedProductPicture = (await GetPicturesByProductIdAsync(product.ParentGroupedProductId, 1)).FirstOrDefault();
            return parentGroupedProductPicture;
        }

        #endregion
    }
}
