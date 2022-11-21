using Api.Services;

namespace Api.Middlewares
{
	public class StopDoSMiddleware
	{
		private readonly RequestDelegate _next;
		public StopDoSMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, DoSGuard guard)
		{
			var headerAuth = context.Request.Headers.Authorization;

			try
			{
				guard.CheckRequest(headerAuth);
				await _next(context);
			}
			catch (TooManyRequestsException)
			{
				context.Response.StatusCode = 429;
				await context.Response.WriteAsJsonAsync("too many Requests, allowed 10 request per second");
			}
		}
	}
	public static class StopDoSMiddlewareExtensions
	{
		public static IApplicationBuilder UseAntiDoSCustom(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<StopDoSMiddleware>();
		}
	}
}
