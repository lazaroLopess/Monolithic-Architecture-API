using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace Monolithic_Architecture_API.Middleware
{
    public static class ApiExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if(contextFeature != null)
                    {
                        await context.Response.WriteAsync(JsonSerializer.Serialize
                        (
                            new
                            {
                                context.Response.StatusCode,
                                contextFeature.Error.Message,
                                Trace = contextFeature.Error.StackTrace
                            })
                        );
                    }
                });
            });
        }
    }
}
