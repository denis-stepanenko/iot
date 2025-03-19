namespace IOTAPI.Models;

public class Range : Component
{
    public required int Min { get; set; }
    public required int Max { get; set; }
    public int Step { get; set; }
}
