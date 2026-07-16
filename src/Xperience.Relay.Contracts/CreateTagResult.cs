namespace Xperience.Relay.Contracts;

public class CreateTagResult
{
    public int TagId { get; set; }
    public Guid TagGuid { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string TagTitle { get; set; } = string.Empty;
}
