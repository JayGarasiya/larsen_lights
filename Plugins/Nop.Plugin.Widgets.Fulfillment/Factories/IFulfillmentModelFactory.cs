using Nop.Plugin.Widgets.Fulfillment.Models;

namespace Nop.Plugin.Widgets.Fulfillment.Factories
{
    /// <summary>
    /// Fulfillment service interface.
    /// </summary>
    public partial interface IFulfillmentModelFactory
    {
        /// <summary>
        /// Prepare three pl records search model
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        Task<ThreePlRecordSearchModel> PrepareThreePlRecordsSearchModelAsync(ThreePlRecordSearchModel searchModel);

        /// <summary>
        /// Prepare three pl records list model async
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        Task<ThreePlRecordListModel> PrepareThreePlRecordsListModelAsync(ThreePlRecordSearchModel searchModel);

        /// <summary>
        /// Prepare three pl orders list model async
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        Task<ThreePlOrderListModel> PrepareThreePlOrdersListModelAsync(ThreePlOrderSearchModel searchModel);

        /// <summary>
        /// Prepare three pl shipping search model
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        ThreePlShippingMethodSearchModel PrepareThreePlShippingSearchModel(ThreePlShippingMethodSearchModel searchModel);

        /// <summary>
        /// Prepare three pl shipping list model async
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        Task<ThreePlShippingMethodListModel> PrepareThreePlShippingListModelAsync(ThreePlShippingMethodSearchModel searchModel);
    }
}
