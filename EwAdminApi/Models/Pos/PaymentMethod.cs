namespace EwAdminApi.Models.Pos;

public class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    public string? PaymentMethodCode { get; set; }

    public string? PaymentMethodName { get; set; }

    public int DisplayIndex { get; set; }

    public bool Enabled { get; set; }

    public int AccountId { get; set; }
    
    public string? LinkedGateway { get; set; }
    
    public bool? IsNonSalesPayment { get; set; }

    public bool? IsCashPayment { get; set; }
    
}