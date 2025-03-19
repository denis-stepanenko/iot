using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IOTAPI.Hubs
{
    [Authorize]
    public class TestHTTPHub : Hub
    {
    }
}
