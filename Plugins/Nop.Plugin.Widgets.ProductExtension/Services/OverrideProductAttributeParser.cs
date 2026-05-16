using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Nop.Plugin.Widgets.ProductExtension.Services
{
    /// <summary>
    /// Product attribute parser
    /// </summary>
    public class OverrideProductAttributeParser : ProductAttributeParser
    {
        #region Ctor

        public OverrideProductAttributeParser(ICurrencyService currencyService,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IProductAttributeService productAttributeService,
            IRepository<ProductAttributeValue> productAttributeValueRepository,
            IWorkContext workContext) : base(currencyService,
            downloadService,
            localizationService,
            productAttributeService,
            productAttributeValueRepository,
            workContext) { }

        #endregion

        #region Methods

        #region Product attributes

        /// <summary>
        /// Check whether condition of some attribute is met (if specified). Return "null" if not condition is specified
        /// </summary>
        /// <param name="pam">Product attribute</param>
        /// <param name="selectedAttributesXml">Selected attributes (XML format)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public override async Task<bool?> IsConditionMetAsync(ProductAttributeMapping pam, string selectedAttributesXml)
        {
            ArgumentNullException.ThrowIfNull(pam);

            var conditionAttributeXml = pam.ConditionAttributeXml;
            if (string.IsNullOrEmpty(conditionAttributeXml))
                //no condition
                return null;

            //load an attribute this one depends on
            var dependOnAttribute = (await base.ParseProductAttributeMappingsAsync(conditionAttributeXml)).FirstOrDefault();
            if (dependOnAttribute == null)
                return true;

            var valuesThatShouldBeSelected = base.ParseValues(conditionAttributeXml, dependOnAttribute.Id)
                //a workaround here:
                //ConditionAttributeXml can contain "empty" values (nothing is selected)
                //but in other cases (like below) we do not store empty values
                //that's why we remove empty values here
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var selectedValues = base.ParseValues(selectedAttributesXml, dependOnAttribute.Id);

            //radio list condition for any contain if multiple selection
            if (dependOnAttribute.AttributeControlType == AttributeControlType.RadioList && valuesThatShouldBeSelected.Count > 1)
                return valuesThatShouldBeSelected.Intersect(selectedValues).Any();

            if (valuesThatShouldBeSelected.Count != selectedValues.Count)
                return false;

            //compare values
            var allFound = true;
            foreach (var t1 in valuesThatShouldBeSelected)
            {
                var found = false;
                foreach (var t2 in selectedValues)
                    if (t1 == t2)
                        found = true;
                if (!found)
                    allFound = false;
            }

            return allFound;
        }

        #endregion

        #endregion
    }
}