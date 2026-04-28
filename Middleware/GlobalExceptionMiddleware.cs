using System.Net;

namespace HoneyBack.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Method} {Path}", ctx.Request.Method, ctx.Request.Path);

            if (!ctx.Response.HasStarted)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new { mensaje = "Error interno del servidor." });
            }
        }
    }
}
