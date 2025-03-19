using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Services;
using IdentityServer.Data;
using IdentityServer.Models;
using IdentityServer.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        string connectionString = Environment.GetEnvironmentVariable("APP_POSTGRESQL_CONNECTION_STRING")
                ?? throw new Exception("PostgreSQL connection string is not specified");

        string ? assemblyName = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        builder.Services.AddDbContext<ApplicationDb>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDb>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
                //options.UserInteraction.LogoutUrl = "http://localhost:5173";
                //options.Authentication.RequireCspFrameSrcForSignout = false;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            // Add custom claims
            .AddProfileService<CustomProfileService>()
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(assemblyName));

                // Automatic token cleanup
                // whether expired grants and pushed authorization requests will be cleaned up from database
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600;
            });

        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        //builder.Services.AddHttpLogging(o => { });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.Use((context, next) =>
        {
            context.Request.Scheme = "https";
            return next(context);
        });

        // Added
        //app.UseHttpLogging();

        app.Use((context, next) =>
        {            
            Log.Information("Request: " + $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}");
            return next(context);
        });

        // https://stackoverflow.com/questions/51912757/identity-server-is-keep-showing-showing-login-user-is-not-authenticated-in-c
        app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

        app.UseForwardedHeaders();

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        InitializeDatabase(app);

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();


        app.Use(async (context, next) =>
        {
            if (app.Environment.IsDevelopment())
            {
                context.Response.Headers.Append("Content-Security-Policy",
                    "default-src 'self' localhost:* 'unsafe-inline' data: ws: wss: localhost:*; " +
                    "connect-src 'self' localhost;" +
                    "frame-ancestors 'self' localhost; " +
                    "script-src 'self' 'unsafe-inline' localhost; " +
                    "style-src-elem 'self' fonts.googleapis.com; " +
                    "font-src 'self' fonts.gstatic.com;");
            }
            else
            {
                context.Response.Headers.Append("Content-Security-Policy",
                    "style-src-elem 'self' fonts.googleapis.com; " +
                    "font-src 'self' fonts.gstatic.com;");
            }
            await next();
        });


        return app;
    }

    private static void InitializeDatabase(IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope())
        {
            serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
            serviceScope.ServiceProvider.GetRequiredService<ApplicationDb>().Database.Migrate();
        }
    }

}