using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using ChatApplicationApi.Contexts;
using ChatApplicationApi.Hubs;
using ChatApplicationApi.Middlewares;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((configBuilder) =>
{
    configBuilder.Sources.Clear();
    DotEnv.Load();
    configBuilder.AddEnvironmentVariables();
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy/*.WithOrigins(
            builder.Configuration.GetValue<string>("CLIENT_ORIGIN_URL"))*/
            /*.WithHeaders(new string[] {
                HeaderNames.ContentType,
                HeaderNames.Authorization,
            })*/
            .AllowAnyOrigin()
            .AllowAnyHeader()
            //.WithMethods("GET")
            .AllowAnyMethod()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
    });
});

builder.Services.AddSignalR(e => {
    e.MaximumReceiveMessageSize = 536870888;
});
builder.Services.AddControllers();

builder.Services.AddDbContext<ChatApplicationDataContext>(
    o => o.UseNpgsql(builder.Configuration.GetValue<string>("POSTGRESQL_CONNECTION_STRING"))
    );

builder.Host.ConfigureServices((services) =>
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)/*.AddAuthorization(options =>
    {
        var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
            JwtBearerDefaults.AuthenticationScheme);

        defaultAuthorizationPolicyBuilder =
            defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();

        options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
    })*/
        .AddJwtBearer(options =>
        {
            var audience =
                  builder.Configuration.GetValue<string>("AUTH0_AUDIENCE");

            options.Authority =
                  $"https://{builder.Configuration.GetValue<string>("AUTH0_DOMAIN")}/";
            options.Audience = audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuerSigningKey = true
            };
        })
);

var app = builder.Build();

var requiredVars =
    new string[] {
          "PORT",
          "CLIENT_ORIGIN_URL",
          "AUTH0_DOMAIN",
          "AUTH0_AUDIENCE",
          "POSTGRESQL_CONNECTION_STRING",
          "REDIS_CONNECTION_STRING"
    };

foreach (var key in requiredVars)
{
    var value = app.Configuration.GetValue<string>(key);

    if (value == "" || value == null)
    {
        throw new Exception($"Config variable missing: {key}.");
    }
}

app.Urls.Add(
    $"https://+:{app.Configuration.GetValue<string>("PORT")}");

app.MapHub<ChatHub>("hubs/chathub");
app.MapControllers();
app.UseCors();

app.UseGetTokenForWebsokets();
app.UseAuthentication();
app.UseAuthorization();
app.UseGetUserFromToken();

/*
app.UseGetTokenForWebsokets();
app.UseAuthentication();
app.UseAuthorization();
app.UseGetUserFromToken();
*/
app.Run();
