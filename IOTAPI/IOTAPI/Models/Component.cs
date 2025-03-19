using System.Text.Json.Serialization;

namespace IOTAPI.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Toggle), "toggle")]
[JsonDerivedType(typeof(Range), "range")]
[JsonDerivedType(typeof(Indicator), "indicator")]
public class Component
{
    public required string Title { get; set; }
    public required string Topic { get; set; }
    public DateTime CreatedAt { get; set; }
}
