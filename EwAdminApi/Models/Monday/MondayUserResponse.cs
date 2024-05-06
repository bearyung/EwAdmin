namespace EwAdminApi.Models.Monday;

public record MondayUserResponse
{
    public MondayUserData? Data { get; set; }
}

public record MondayUserData
{
    public MondayUser? Me { get; set; }
}

public record MondayUser
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}