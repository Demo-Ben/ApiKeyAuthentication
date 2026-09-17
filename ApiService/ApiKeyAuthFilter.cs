using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static System.Net.WebRequestMethods;

namespace ApiService
{

//The relationship between HttpContext and ActionExecutingContext is a parent-to - child containment relationship.
//┌────────────────────────────────────────────────────────┐
//│ HttpContext(The entire HTTP request/response lifetime) │
//│                                                        │
//│   ┌────────────────────────────────────────────────┐   │
//│   │ ActionExecutingContext                         │   │
//│   │ (Only lives while the MVC Action is executing) │   │
//│   │                                                │   │
//│   │  • context.ActionDescriptor                    │   │
//│   │  • context.ActionArguments                     │   │
//│   │  • context.Result                              │   │
//│   │  • context.HttpContext ──(Points back up)──────┼───┘
//│   └────────────────────────────────────────────────┘   │
//└────────────────────────────────────────────────────────┘

    // Implements IAsyncActionFilter to run custom validation logic before and after controller actions execute
    public class ApiKeyAuthFilter : IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-API-KEY";

        // Core execution method required by the filter interface, handling asynchronous interception
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Extract the API key header value from the incoming HTTP request headers collection
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                // Short-circuits pipeline execution and returns HTTP 401 Unauthorized if the header is missing entirely
                context.Result = new UnauthorizedObjectResult("API Key missing from request header.");
                return;
            }

            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = configuration.GetValue<string>("ApiKeySettings:ApiKey");

            // Validates whether the extracted client key matches the expected server-side configuration key
            if (!apiKey.Equals(extractedApiKey))
            {
                // Short-circuits pipeline execution and returns HTTP 401 Unauthorized if keys do not match
                context.HttpContext.Items["CustomStatusCode"] = StatusCodes.Status401Unauthorized;
                context.Result = new UnauthorizedObjectResult("Invalid API Key.");
                return;
            }

            // Proceeds to execute the next filter in the pipeline or the actual controller action method
            // if validation succeeds
            await next();
        }
    }
}
