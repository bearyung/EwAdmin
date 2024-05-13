using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class ShopWorkdayPeriodDetailEvent(ShopWorkdayPeriodDetail? shopWorkdayPeriodDetailMessage)
{
    public ShopWorkdayPeriodDetail? ShopWorkdayPeriodDetailMessage { get; } = shopWorkdayPeriodDetailMessage;
}