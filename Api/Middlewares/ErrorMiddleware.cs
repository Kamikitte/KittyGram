using Api.Exceptions;

namespace Api.Middlewares;

public sealed class ErrorMiddleware
{
    private readonly RequestDelegate next;

    public ErrorMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(ex.Message);
        }
    }
}

public static class ErrorMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalErrorWrapper(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ErrorMiddleware>();
}