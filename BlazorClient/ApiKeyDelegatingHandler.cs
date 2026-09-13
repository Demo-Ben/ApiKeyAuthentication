namespace BlazorClient
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    public class ApiKeyDelegatingHandler : DelegatingHandler
    {
        private readonly string _apiKey;

        public ApiKeyDelegatingHandler(IConfiguration configuration)
        {
            // Retrieves the key dynamically, falling back to an empty string if configuration is missing
            _apiKey = configuration["ApiSettings:ApiKey"] ?? string.Empty;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Verifies that the API key exists and is not empty before attempting injection
            if (!string.IsNullOrEmpty(_apiKey))
            {
                // Appends the custom authentication header with the secret key to the outgoing request headers collection
                request.Headers.Add("X-API-KEY", _apiKey);
            }

            // Forwards the modified HTTP request down the pipeline to the next handler or the actual network transport
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
