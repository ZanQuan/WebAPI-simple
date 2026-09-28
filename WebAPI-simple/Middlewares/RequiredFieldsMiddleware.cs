using System.Text.Json;

namespace WebAPI_simple.Middlewares
{
    public class RequiredFieldsMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] RequiredBookFields = { "title", "publisherID", "authorIds" };

        public RequiredFieldsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method) &&
                context.Request.Path.Equals("/api/Books/add-book", StringComparison.OrdinalIgnoreCase))
            {
                context.Request.EnableBuffering();   
                try
                {
                    using var doc = await JsonDocument.ParseAsync(context.Request.Body);
                    context.Request.Body.Position = 0;   

                    if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    {
                        await WriteError(context, "Body phải là một đối tượng JSON", null);
                        return;
                    }

                    var present = doc.RootElement.EnumerateObject()
                        .Select(p => p.Name.ToLowerInvariant()).ToHashSet();
                    var missing = RequiredBookFields
                        .Where(f => !present.Contains(f.ToLowerInvariant())).ToList();

                    if (missing.Count > 0)
                    {
                        await WriteError(context, "Thiếu các trường bắt buộc", missing);
                        return;
                    }
                }
                catch (JsonException)
                {
                    await WriteError(context, "Body không phải JSON hợp lệ hoặc bị trống", null);
                    return;
                }
            }

            await _next(context);
        }

        private static async Task WriteError(HttpContext context, string message, List<string>? missingFields)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message, missingFields });
        }
    }
}