using Api.Services;
using Common.Constants;
using Common.Extensions;

namespace Api.Middlewares;

public sealed class TokenValidatorMiddleware
{
    private readonly RequestDelegate next;

    public TokenValidatorMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, AuthService authService)
    {
        var isOk = true;
        var sessionId = context.User.GetClaimValue<Guid>(ClaimNames.SessionId);
        if (sessionId != Guid.Empty)
        {
            var session = await authService.GetSessionById(sessionId);
            if (!session.IsActive)
            {
                isOk = false;
                context.Response.Clear();
                context.Response.StatusCode = 401;
            }
        }

        if (isOk)
        {
            await next(context);
        }
    }
}

public static class TokenValidatorMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenValidator(this IApplicationBuilder builder) =>
        builder.UseMiddleware<TokenValidatorMiddleware>();
}