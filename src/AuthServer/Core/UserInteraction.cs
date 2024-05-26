namespace AuthServer.Core;

public class UserInteraction
{
    public required string LoginUri { get; set; }
    public required string ConsentUri { get; set; }
    public required string AccountSelectionUri { get; set; }
}