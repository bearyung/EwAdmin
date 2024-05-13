using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class ShopWorkdayDetailEvent(ShopWorkdayDetail? shopWorkdayDetailMessage)
{
    public ShopWorkdayDetail? ShopWorkdayDetailMessage { get; } = shopWorkdayDetailMessage;
}