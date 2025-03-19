using Microsoft.AspNetCore.Mvc;

namespace IOTAPI.ApiKeyAuthorization
{
    public class ApiKeyAttribute : ServiceFilterAttribute
    {
        public ApiKeyAttribute() : base(typeof(ApiKeyAuthorizationFilter))
        { }
    }
}
