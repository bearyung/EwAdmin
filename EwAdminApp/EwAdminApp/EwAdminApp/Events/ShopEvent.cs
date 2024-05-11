using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class ShopEvent(Shop? shop)
{
    public Shop? Shop { get; } = shop;
}