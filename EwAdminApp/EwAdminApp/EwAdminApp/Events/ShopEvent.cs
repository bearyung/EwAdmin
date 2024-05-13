using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class ShopEvent(Shop? shopMessage)
{
    public Shop? ShopMessage { get; } = shopMessage;
}