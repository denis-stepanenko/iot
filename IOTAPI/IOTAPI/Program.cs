using IOTAPI.ApiKeyAuthorization;
using IOTAPI.Data;
using IOTAPI.Data.Repos;
using IOTAPI.Hubs;
using IOTAPI.MQTTBroker;
using IOTAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddMQTTBroker();

builder.Services.AddSignalR(e => e.EnableDetailedErrors = true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlDocsPath = Path.Combine(AppContext.BaseDirectory, typeof(Program).Assembly.GetName().Name + ".xml");
    options.IncludeXmlComments(xmlDocsPath);
});

string connectionString = Environment.GetEnvironmentVariable("APP_REDIS_CONNECTION_STRING")
    ?? throw new ArgumentException("Не указана строка подключения к Redis");

builder.Services.AddSingleton(new RedisContext(connectionString));
builder.Services.AddScoped<ComponentRepo>();
builder.Services.AddScoped<ReadingRepo>();
builder.Services.AddScoped<TemperatureRepo>();

builder.Services.AddSingleton<WebSocketService>();

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
builder =>
{
    builder.WithOrigins("http://localhost:3000")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials();
}));

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = Environment.GetEnvironmentVariable("APP_AUTHORITY");
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.ValidIssuer = Environment.GetEnvironmentVariable("APP_AUTHORITY");
        options.TokenValidationParameters.ValidateIssuerSigningKey = false;
        options.TokenValidationParameters.SignatureValidator = delegate (string token, TokenValidationParameters parameters)
        {
            var jwt = new JsonWebToken(token);

            return jwt;
        };

        // Authentication for SignalR Hubs
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireClaim("scope", "iotapi")
                .Build();
});

builder.Services.AddApiKeyAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("CorsPolicy");
}

app.UseMqttBroker();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.UseWebSockets();

app.MapHub<ClientHub>("hubs/clients");
app.MapHub<ReadingHub>("hubs/readings");

app.MapHub<TemperatureHub>("hubs/temperature");
app.MapHub<TestHTTPHub>("hubs/testHTTP");
app.MapHub<TestWSHub>("hubs/testWS");

app.Run();

