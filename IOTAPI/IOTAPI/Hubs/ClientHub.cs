using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MQTTnet.AspNetCore;

namespace IOTAPI.Hubs;

[Authorize]
public class ClientHub : Hub
{
    private readonly MqttHostedServer _server;

    public ClientHub(MqttHostedServer server)
    {
        _server = server;
    }

    public async Task GetAll()
    {
        var clients = await _server.GetClientsAsync();
        await Clients.All.SendAsync("GetAll", clients);
    }

}
