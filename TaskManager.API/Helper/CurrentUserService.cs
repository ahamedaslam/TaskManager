using System.Security.Claims;

//Abstract. In the ASP.NET Core Web API, the HttpContextAccessor class is a
//component that provides access to the current HTTP request and response context.
//It allows you to access various aspects of the HTTP request and response, such as headers, cookies, query parameters, and user claims.

public class CurrentUserService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public CurrentUserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }
    
    public string? GetUserId => _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? GetRole => _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
    public string? GetTenantId => _contextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;
}
