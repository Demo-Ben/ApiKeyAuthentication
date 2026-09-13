namespace DemoApi.Middleware
{
    // Custom ASP.NET Core middleware that intercepts incoming HTTP requests to enforce API key authentication globally
    public class ApiKeyMiddleware
    {
        // Reference to the next middleware component in the request processing pipeline
        private readonly RequestDelegate _next;

        // Constant defining the HTTP header name used to pass the API key from clients
        private const string ApiKeyHeaderName = "X-API-KEY";

        // Constructor receives the next delegate in the pipeline during application startup
        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Main execution method invoked automatically by the runtime for every incoming HTTP request
        public async Task InvokeAsync(HttpContext context)
        {
            // Bypasses API key validation for Swagger endpoints to allow easy API documentation access
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            // Attempts to retrieve the API key header value from the incoming request headers collection
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                // Short-circuits the pipeline with a 401 Unauthorized response if the header is completely missing
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API Key was not provided. (Header missing)");
                return;
            }

            // Dynamically resolves IConfiguration from the request container to access server-side configuration settings
            var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

            // Retrieves the valid pre-configured secret API key from application configuration (e.g., appsettings.json)
            var apiKey = configuration.GetValue<string>("ApiKeySettings:ApiKey");

            // Validates whether the client's provided API key matches the expected server-side secret key
            if (!apiKey.Equals(extractedApiKey))
            {
                // Short-circuits the pipeline with a 401 Unauthorized response if the keys do not match
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized client. (Invalid API Key)");
                return;
            }

            // Forwards the request down to the next middleware or controller endpoint if authentication succeeds
            await _next(context);
        }
    }
}
