using System;

namespace IOTAPI.Models;

public class MQTTBrokerSettings
{
    public int TCPPort { get; set; }
    public int WSPort { get; set; }
    // public List<User> Users { get; set; } = new List<User>();
    public required string DefaultUserName { get; set; }
    public required string DefaultPassword { get; set; }
}
