using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.ShipmentTracking.Domain
{
    /// <summary>
    /// Represents an extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert to select list
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <param name="enumObj">Enum</param>
        /// <param name="markCurrentAsSelected">Mark current value as selected</param>
        /// <param name="valuesToExclude">Values to exclude</param>
        /// <param name="useLocalization">Localize</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the selectList
        /// </returns>
        public static async Task<SelectList> ToSelectListWithStringValuesAsync<TEnum>(this TEnum enumObj,
           bool markCurrentAsSelected = true, int[] valuesToExclude = null, bool useLocalization = true) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("An Enumeration type is required.", nameof(enumObj));

            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            var values = await Enum.GetValues(typeof(TEnum)).OfType<TEnum>().Where(enumValue =>
                    valuesToExclude == null || !valuesToExclude.Contains(Convert.ToInt32(enumValue)))
                .SelectAwait(async enumValue => new
                {
                    ID = enumValue.ToString(),
                    Name = useLocalization
                        ? await localizationService.GetLocalizedEnumAsync(enumValue)
                        : CommonHelper.SplitCamelCaseWord(enumValue.ToString())
                }).ToListAsync();

            object selectedValue = null;
            if (markCurrentAsSelected)
                selectedValue = enumObj.ToString();
            return new SelectList(values, "ID", "Name", selectedValue);
        }

        /// <summary>
        /// Is equal to
        /// </summary>
        /// <param name="billing">Address</param>
        /// <param name="shipping">Address</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the selectList
        /// </returns>
        public static bool IsEqualTo(this Address billing, Address shipping)
        {
            foreach (var prop in typeof(Address).GetProperties())
            {
                if (prop.Name.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (prop.Name.Equals("CreatedOnUtc", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (prop.Name.Equals("CustomAttributes", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var value1 = prop.GetValue(billing);
                var value2 = prop.GetValue(shipping);

                if (value1 is null && value2 is null)
                    continue;

                if (value1 is null && value2 is not null)
                    return false;

                if (value1 is not null && value2 is null)
                    return false;

                if (!value1.Equals(value2))
                {
                    return false;
                }
            }
            return true;
        }
    }
}