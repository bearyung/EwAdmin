using EwAdmin.Common.Models.WebAdmin;

namespace EwAdminApp.Events;

public class WebAdminBrandEvent(BrandMaster? brandMessage)
{
    public BrandMaster? BrandMessage { get; } = brandMessage;
}