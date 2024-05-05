namespace EwAdminApi.Extensions;

public class CustomErrorRequestResultDto
{
    public string Type { get; set; }
    public string Title { get; set; }
    public int Status { get; set; }
    public object Errors { get; set; }
    public string TraceId { get; set; }
}