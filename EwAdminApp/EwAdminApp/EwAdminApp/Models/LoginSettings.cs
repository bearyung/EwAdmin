namespace EwAdminApp.Models;

public record LoginSettings
{
    public string? ApiKey { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
}