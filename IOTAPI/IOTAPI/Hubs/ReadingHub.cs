using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IOTAPI.Hubs;

[Authorize]
public class ReadingHub : Hub
{
}
