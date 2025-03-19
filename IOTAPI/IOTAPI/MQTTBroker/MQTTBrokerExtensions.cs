using System.Text;
using System.Text.Json;
using IOTAPI.Data.Repos;
using IOTAPI.Hubs;
using IOTAPI.Models;
using Microsoft.AspNetCore.SignalR;
using MQTTnet.AspNetCore;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace IOTAPI.MQTTBroker;

public static class MQTTBrokerExtensions
{
    static MQTTBrokerSettings _settings;
    static IHubContext<ClientHub> _clientHub;
    static IHubContext<ReadingHub> _readingHub;
    static MqttHostedServer _server;

    static IServiceScopeFactory _serviceScopeFactory;

    static ComponentRepo _componentRepo;
    static ReadingRepo _readingRepo;

    static string _retainedMessagesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "RetainedMessages.json");

    public static void AddMQTTBroker(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedMqttServer(mqttServer => mqttServer.WithDefaultEndpoint())
            .AddMqttConnectionHandler()
            .AddConnections();

        _settings = builder.Configuration.GetSection("MQTTBrokerSettings").Get<MQTTBrokerSettings>()
            ?? throw new ArgumentNullException("Не указаны настройки для MQTT брокера");

        builder.WebHost.ConfigureKestrel(o =>
        {
            o.ListenAnyIP(_settings.TCPPort, l => l.UseMqtt());
            o.ListenAnyIP(_settings.WSPort);
        });
    }

    public static void UseMqttBroker(this WebApplication app)
    {
        app.UseRouting();

        app.MapConnectionHandler<MqttConnectionHandler>(
        "/mqtt",
        httpConnectionDispatcherOptions =>
            httpConnectionDispatcherOptions.WebSockets.SubProtocolSelector =
                protocolList =>
                    protocolList.FirstOrDefault() ?? string.Empty);

        _clientHub = app.Services.GetRequiredService<IHubContext<ClientHub>>();
        _readingHub = app.Services.GetRequiredService<IHubContext<ReadingHub>>();

        app.UseMqttServer(server =>
        {
            server.StartedAsync += StartedAsync;
            server.ValidatingConnectionAsync += ValidateConnectionAsync;
            server.InterceptingSubscriptionAsync += InterceptSubscriptionAsync;
            server.InterceptingPublishAsync += InterceptApplicationMessagePublishAsync;
            server.ClientDisconnectedAsync += ClientDisconnectedAsync;
            server.ClientAcknowledgedPublishPacketAsync += ClientAcknowledgedPublishPacketAsync;
            server.ClientConnectedAsync += ClientConnectedAsync;
            server.LoadingRetainedMessageAsync += LoadingRetainedMessageAsync;
            server.RetainedMessageChangedAsync += RetainedMessageChangedAsync;
            server.RetainedMessagesClearedAsync += RetainedMessagesClearedAsync;
        });

        _server = app.Services.GetRequiredService<MqttHostedServer>();

        _serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        _componentRepo = scope.ServiceProvider.GetRequiredService<ComponentRepo>();
        _readingRepo = scope.ServiceProvider.GetRequiredService<ReadingRepo>();

        app.MapGet("/api/client", async (MqttHostedServer server) =>
        {
            var clients = await server.GetClientsAsync();

            return TypedResults.Ok(clients);
        });

        app.MapGet("/api/retainedMessage", async () =>
        {
            if (!File.Exists(_retainedMessagesFilePath))
            {
                return TypedResults.Ok(new List<MqttRetainedMessage>());
            }

            using var stream = File.OpenRead(_retainedMessagesFilePath);

            var models = await JsonSerializer.DeserializeAsync<List<MqttRetainedMessage>>(stream)
                ?? new List<MqttRetainedMessage>();

            return TypedResults.Ok(models);
        });
    }

    static Task StartedAsync(EventArgs arg)
    {
        Console.WriteLine($"Server started with TCP endpoint on {_settings.TCPPort} port and {_settings.WSPort} port with \"/mqtt\" path");
        return Task.CompletedTask;
    }

    static async Task ClientConnectedAsync(ClientConnectedEventArgs args)
    {
        Console.WriteLine($"Client '{args.ClientId}' connected");

        var clients = await _server.GetClientsAsync();
        await _clientHub.Clients.All.SendAsync("ClientsChanged", clients);
    }

    static async Task ClientDisconnectedAsync(ClientDisconnectedEventArgs args)
    {
        Console.WriteLine($"Client '{args.ClientId}' disconnected");

        var clients = await _server.GetClientsAsync();
        await _clientHub.Clients.All.SendAsync("ClientsChanged", clients);
    }

    static async Task ClientAcknowledgedPublishPacketAsync(ClientAcknowledgedPublishPacketEventArgs args)
    {
        Console.WriteLine($"Client '{args.ClientId}' acknowledged packet {args.PublishPacket.PacketIdentifier} with topic '{args.PublishPacket.Topic}'");

        // It is also possible to read additional data from the client response. This requires casting the response packet.
        var qos1AcknowledgePacket = args.AcknowledgePacket as MqttPubAckPacket;
        Console.WriteLine($"QoS 1 reason code: {qos1AcknowledgePacket?.ReasonCode}");

        var qos2AcknowledgePacket = args.AcknowledgePacket as MqttPubCompPacket;
        Console.WriteLine($"QoS 2 reason code: {qos1AcknowledgePacket?.ReasonCode}");

        await Task.CompletedTask;
    }

    static async Task ValidateConnectionAsync(ValidatingConnectionEventArgs args)
    {
        // if (!_settings.Users.Any(x => x.UserName == args.UserName && x.Password == args.Password))
        // {
        //     args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
        // }

        if (!(_settings.DefaultUserName == args.UserName && _settings.DefaultPassword == args.Password))
        {
            args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
        }

        await Task.CompletedTask;
    }

    static Task InterceptSubscriptionAsync(InterceptingSubscriptionEventArgs args)
    {
        try
        {
            args.ProcessSubscription = true;

            Console.WriteLine($"New subscription: ClientId = {args.ClientId}, Topic = {args.TopicFilter.Topic}");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Subscription failed: ClientId = {args.ClientId}, Topic = {args.TopicFilter.Topic}, Error = {ex.Message}");
            return Task.FromException(ex);
        }
    }

    static async Task InterceptApplicationMessagePublishAsync(InterceptingPublishEventArgs args)
    {
        // try
        // {
        //     args.ProcessPublish = true;

        //     var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
        //     Console.WriteLine($"Message: ClientId = {args.ClientId}, Topic = {args.ApplicationMessage.Topic}, Payload = {payload}, QoS = {args.ApplicationMessage?.QualityOfServiceLevel}, Retain-Flag = {args.ApplicationMessage?.Retain}");

        //     return Task.CompletedTask;
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine("An error occurred: {Exception}.", ex);
        //     return Task.FromException(ex);
        // }

        args.ProcessPublish = true;

        string payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
        string topic = args.ApplicationMessage.Topic;

        var component = await _componentRepo.GetAsync(topic);

        if (component != null)
        {
            if (component is Indicator)
            {
                if(double.TryParse(payload, out double value))
                {
                    var reading = await _readingRepo.AddAsync(topic, value);

                    // Оповещаем frontend
                    await _readingHub.Clients.All.SendAsync("ReadingsChanged", topic, reading);
                }
            }
        }
    }

    static async Task LoadingRetainedMessageAsync(LoadingRetainedMessagesEventArgs args)
    {
        try
        {
            if (!File.Exists(_retainedMessagesFilePath))
            {
                Console.WriteLine("No retained messages stored yet.");
                return;
            }

            using var stream = File.OpenRead(_retainedMessagesFilePath);

            var models = await JsonSerializer.DeserializeAsync<List<MqttRetainedMessage>>(stream)
                ?? new List<MqttRetainedMessage>();

            var retainedMessages = models.Select(m => m.ToApplicationMessage()).ToList();

            args.LoadedRetainedMessages = retainedMessages;

            Console.WriteLine("Retained messages loaded.");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    static async Task RetainedMessageChangedAsync(RetainedMessageChangedEventArgs args)
    {
        try
        {
            var models = args.StoredRetainedMessages.Select(MqttRetainedMessage.Create);

            var buffer = JsonSerializer.SerializeToUtf8Bytes(models);
            await File.WriteAllBytesAsync(_retainedMessagesFilePath, buffer);

            Console.WriteLine("Retained messages saved.");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    static async Task RetainedMessagesClearedAsync(EventArgs args)
    {
        File.Delete(_retainedMessagesFilePath);

        await Task.CompletedTask;
    }
}
