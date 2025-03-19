namespace IOTAPI.ApiKeyAuthorization
{
    public interface IApiKeyValidator
    {
        bool IsValid(string key);
    }
}
