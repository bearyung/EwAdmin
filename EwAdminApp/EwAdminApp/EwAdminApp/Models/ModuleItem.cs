namespace EwAdminApp.Models;

public class ModuleItem
{
    public string? DisplayName { get; set; }
    public UserModuleEnum Module { get; set; }
    
    public string? IconResourceKey { get; set; }
    public bool IsAccessible { get; set; }
}