namespace EwAdmin.Common.Models.Setting;

public record LoginUserResponse
{
    public LoginUserData? Data { get; set; }
}

public record LoginUserData
{
    public LoginUser? Me { get; set; }
}

public record LoginUser
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}