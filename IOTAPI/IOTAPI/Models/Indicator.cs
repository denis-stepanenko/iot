namespace IOTAPI.Models;

public class Indicator : Component
{
    public IEnumerable<Reading> Readings { get; set; } = new List<Reading>();
}