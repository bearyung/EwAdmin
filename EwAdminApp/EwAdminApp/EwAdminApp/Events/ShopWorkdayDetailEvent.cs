using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class ShopWorkdayDetailEvent (ShopWorkdayDetail? shopWorkdayDetail)
{
    public ShopWorkdayDetail? ShopWorkdayDetail { get; } = shopWorkdayDetail;
}