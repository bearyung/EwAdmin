using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class ItemCategoryEvent(ItemCategory? itemCategoryMessage)
{
    public ItemCategory? ItemCategoryMessage { get; } = itemCategoryMessage;    
}