using ChatApplicationApi.Components;
using ChatApplicationApi.ModelClasses;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using RestSharp;

namespace ChatApplicationApi.Middlewares
{
    public class GetUserFromTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public GetUserFromTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            Console.WriteLine("GetUserFromTokenPrev");

            var endpoint = httpContext.GetEndpoint();

            if (endpoint?.Metadata?.GetMetadata<AuthorizeAttribute>() is object)
            {
                string token = (string)(httpContext.Request.Headers["Authorization"]);
                token = token.Split("Bearer ")[1];

                var client = new RestClient("https://dev-qffze3gmn7378pe8.us.auth0.com");
                var request = new RestRequest("userinfo");

                request.AddHeader("Content-type", "application/json");
                request.AddHeader("Authorization", $"Bearer {token}");

                RestResponse response = client.ExecutePost(request);

                Auth0UserInfo responseContent = JsonConvert.DeserializeObject<Auth0UserInfo>(response.Content);

                if (responseContent.Error != null) {
                    throw new Exception("Error occured in auth0");
                }

                httpContext.Request.Headers.Add("User", response.Content);
            }

            Console.WriteLine("GetUserFromToken");

            await _next(httpContext);
        }
    }

    public static class GetUserFromTokenExtensions
    {
        public static IApplicationBuilder UseGetUserFromToken(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GetUserFromTokenMiddleware>();
        }
    }
}
