using System.Net.WebSockets;
using System.Text;

namespace IOTAPI.Services
{
    public class WebSocketService
    {
        public Dictionary<string, WebSocket> WebSockets { get; set; } = new();

        public event MessageReceivedEventHandler OnMessageReceivedEvent;
        public delegate void MessageReceivedEventHandler(object sender, WebsocketEventArgs e);

        public event DisconnectedEventHandler OnDisconnectedEvent;
        public delegate Task DisconnectedEventHandler(object sender, DisconnectedEventArgs e);

        public async Task HandleConnection(WebSocket ws, string deviceName)
        {
            WebSockets.Add(deviceName, ws);

            while(ws.State == WebSocketState.Open)
            {
                try
                {
                    string message = await ReceiveMessageAsync(ws);
                    OnMessageReceivedEvent(ws, new WebsocketEventArgs(deviceName, message));
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine($"{deviceName}: " + ex.Message);
                    await OnDisconnectedEvent(ws, new DisconnectedEventArgs(deviceName));
                    break;
                }
                catch(Exception ex) when (ex.Message == $"Client requested to close connection")
                {
                    Console.WriteLine(ex.Message);
                    await OnDisconnectedEvent(ws, new DisconnectedEventArgs(deviceName));
                    break;
                }
            }

            WebSockets.Remove(deviceName);
        }

        public async Task SendMessageAsync(WebSocket ws, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task<string> ReceiveMessageAsync(WebSocket ws)
        {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[1024]);
            WebSocketReceiveResult? result = null;

            using (var memoryStream = new MemoryStream())
            {
                do
                {
                    result = await ws.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, default);
                        throw new Exception("Client requested to close connection");
                    }

                    if (ws.State == WebSocketState.CloseReceived)
                    {
                        await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
                    }

                    memoryStream.Write(buffer.Array, buffer.Offset, result.Count);

                } while (!result.EndOfMessage);

                memoryStream.Seek(0, SeekOrigin.Begin);

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }

    public class WebsocketEventArgs : EventArgs
    {
        public readonly string DeviceName;
        public readonly string Message;

        public WebsocketEventArgs(string deviceName, string message)
        {
            DeviceName = deviceName;
            Message = message;
        }
    }

    public class DisconnectedEventArgs : EventArgs
    {
        public readonly string DeviceName;

        public DisconnectedEventArgs(string deviceName)
        {
            DeviceName = deviceName;
        }
    }

}