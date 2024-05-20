using EwAdminApp.Models;

namespace EwAdminApp.Events;

public class ModuleItemEvent(ModuleItem? moduleItemMessage)
{
    public ModuleItem? ModuleItemMessage { get; } = moduleItemMessage;
}