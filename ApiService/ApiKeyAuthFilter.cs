using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiService
{

    // Implements IAsyncActionFilter to run custom validation logic before and after controller actions execute
    public class ApiKeyAuthFilter : IAsyncActionFilter
    {

        private const string ApiKeyHeaderName = "X-API-KEY";

        // Core execution method required by the filter interface, handling asynchronous interception
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Attempts to extract the API key header value from the incoming HTTP request headers collection
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                // Short-circuits pipeline execution and returns HTTP 401 Unauthorized if the header is missing entirely
                context.Result = new UnauthorizedObjectResult("API Key missing from request header.");
                return;
            }

            // Resolves the IConfiguration service dynamically from the current request's dependency injection container
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            // Retrieves the pre-configured valid API key from application settings (e.g., appsettings.json)
            var apiKey = configuration.GetValue<string>("ApiKeySettings:ApiKey");

            // Validates whether the extracted client key matches the expected server-side configuration key
            if (!apiKey.Equals(extractedApiKey))
            {
                // Short-circuits pipeline execution and returns HTTP 401 Unauthorized if keys do not match
                context.Result = new UnauthorizedObjectResult("Invalid API Key.");
                return;
            }

            // Proceeds to execute the next filter in the pipeline or the actual controller action method if validation succeeds
            await next();
        }
    }
}
