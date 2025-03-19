namespace IOTAPI.ApiKeyAuthorization
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly IConfiguration _configuration;
        public ApiKeyValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsValid(string key)
        {
            string? apiKey = Environment.GetEnvironmentVariable("APP_API_KEY");

            if(apiKey == null)
                throw new ArgumentException("API key should be specified");

            return key == apiKey;
        }
    }
}
