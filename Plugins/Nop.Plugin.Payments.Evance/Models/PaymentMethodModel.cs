namespace Nop.Plugin.Payments.Evance.Models;

/// <summary>
/// Represents a payment method model
/// </summary>
public class PaymentMethodModel
{
    public string Name { get; set; } 
    public string LogoUrl { get; set; } 
    public string PaymentMethodSystemName { get; set; } 
    public bool Selected { get; set; } 
}

