using System.Net.Http.Headers;
using System.Text;

namespace SmsTest.MockApi.Middlewares;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string ExpectedUsername = "admin";
    private const string ExpectedPassword = "password";

    public BasicAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            context.Response.StatusCode = 401;
            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"SMS Test API\"";
            await context.Response.WriteAsync("Authorization required");
            return;
        }

        var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader!);
        if (!authHeaderValue.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Only Basic authentication is supported");
            return;
        }

        var credentialBytes = Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
        var username = credentials[0];
        var password = credentials.Length > 1 ? credentials[1] : string.Empty;

        if (username != ExpectedUsername || password != ExpectedPassword)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Invalid username or password");
            return;
        }

        await _next(context);
    }
}
