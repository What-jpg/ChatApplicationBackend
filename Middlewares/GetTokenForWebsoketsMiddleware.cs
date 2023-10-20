namespace ChatApplicationApi.Middlewares
{
    public class GetTokenForWebsoketsMiddleware
    {
        private readonly RequestDelegate _next;

        public GetTokenForWebsoketsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            Console.WriteLine("GetTokenForWebsoketsMiddlewarePreva");

            var request = httpContext.Request;

            if (request.Path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase)
                &&
                request.Query.TryGetValue("access_token", out var authorizationToken))
            {
                Console.WriteLine("WTF");
                request.Headers.Add("Authorization", $"Bearer {authorizationToken}");
            }

            Console.WriteLine("GetTokenForWebsoketsMiddleware");

            await _next(httpContext);
        }
    }

    public static class GetTokenForWebsoketsMiddlewareExtensions
    {
        public static IApplicationBuilder UseGetTokenForWebsokets(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GetTokenForWebsoketsMiddleware>();
        }
    }
}
