namespace EwAdminApi.Models.Monday;

public record MondayApiResponse
{
    public MondayData? Data { get; set; }
    public int AccountId { get; set; }
}

public record MondayData
{
    public List<Board>? Boards { get; set; }
}

public record Board
{
    public ItemsPage? ItemsPage { get; set; }
}

public record ItemsPage
{
    public List<Item>? Items { get; set; }
}

public record Item
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<ColumnValue>? ColumnValues { get; set; }
}

public record ColumnValue
{
    public string Id { get; set; }
    public string Value { get; set; }
}