namespace EwAdminApi.Models.Pos;

public class Shop
{
    public int AccountId { get; set; }
    public int ShopId { get; set; }
    public string? Name { get; set; }
    public string? AltName { get; set; }
    public string? Desc { get; set; }
    public string? AltDesc { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? AddressLine4 { get; set; }
    public string? AltAddressLine1 { get; set; }
    public string? AltAddressLine2 { get; set; }
    public string? AltAddressLine3 { get; set; }
    public string? AltAddressLine4 { get; set; }
    public string? Telephone { get; set; }
    public string? Fax { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencySymbol { get; set; }
    public bool Enabled { get; set; }
    public string? ShopCode { get; set; }
}