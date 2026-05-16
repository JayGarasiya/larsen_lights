namespace Nop.Plugin.Payments.Evance.Models
{
    /// <summary>
    /// Represents a base request model 
    /// </summary>
    public class BaseRequestModel
    {
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        public void AddParameter(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && value != null)
            {
                Parameters[key] = value;
            }
        }

    }

}
