namespace IOTAPI.ApiKeyAuthorization
{
    public static class ApiKeyDependencyInjectionExtensions
    {
        public static void AddApiKeyAuthorization(this IServiceCollection services)
        {
            services.AddScoped<ApiKeyAuthorizationFilter>();
            services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
        }
    }
}
