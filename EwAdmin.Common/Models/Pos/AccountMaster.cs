namespace EwAdmin.Common.Models.Pos;

public class AccountMaster
{
    public int AccountId { get; set; }
 
    public string?AccountName { get; set; }
 
    public string?AccountKey { get; set; }
 
    public bool Enabled { get; set; }
 
    public DateTime CreatedDate { get; set; }
 
    public string?CreatedBy { get; set; }
 
    public DateTime ModifiedDate { get; set; }
 
    public string?ModifiedBy { get; set; }
 
}