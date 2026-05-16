using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Logging;

namespace Nop.Plugin.Widgets.Chat.Services;

/// <summary>
/// Represents the plugin service implementation
/// </summary>
public class ChatScriptService
{
    #region Fields
    private readonly ChatScriptSettings _chatScriptSettings;
    private readonly ILogger _logger;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IWorkContext _workContext;
    #endregion

    #region Ctor
    public ChatScriptService(ChatScriptSettings chatScriptSettings,
        ILogger logger,
        IWidgetPluginManager widgetPluginManager,
        IWorkContext workContext)
    {
        _chatScriptSettings = chatScriptSettings;
        _logger = logger;
        _widgetPluginManager = widgetPluginManager;
        _workContext = workContext;
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Executes a function and returns the result, 
    /// with error handling and plugin active check
    /// </summary>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <param name="function">Function to execute</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result of the function
    /// </returns>
    private async Task<TResult> HandleFunctionAsync<TResult>(Func<Task<TResult>> function)
    {
        try
        {
            // Check if the plugin is active for the current customer and store
            if (!await PluginActiveAsync())
                return default;

            // Execute the provided function asynchronously
            return await function();
        }
        catch (Exception exception)
        {
            // Traverse through inner exceptions to get the root cause
            var detailedException = exception;
            do
            {
                detailedException = detailedException.InnerException;
            } while (detailedException?.InnerException != null);

            // Log the error with the exception details
            var error = $"{ChatScriptDefaults.SystemName} error: {Environment.NewLine}{exception.Message}";
            await _logger.ErrorAsync(error, exception, await _workContext.GetCurrentCustomerAsync());

            return default;  // Return default value in case of an error
        }
    }

    /// <summary>
    /// Checks whether the ChatScript plugin is active for the current customer and store
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a boolean indicating if the plugin is active
    /// </returns>
    private async Task<bool> PluginActiveAsync()
    {
        return await _widgetPluginManager.IsPluginActiveAsync(ChatScriptDefaults.SystemName, await _workContext.GetCurrentCustomerAsync());
    }
    #endregion

    #region Methods
    /// <summary>
    /// Prepares the installation script based on the plugin's settings
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the installation script as a string
    /// </returns>
    public async Task<string> PrepareScriptAsync()
    {
        return await HandleFunctionAsync(() => Task.FromResult(_chatScriptSettings.Script));
    }
    #endregion
}