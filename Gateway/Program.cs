using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var authSettings = builder.Configuration.GetSection("AuthSettings");
var secretKey = authSettings["SecretKey"];
var issuer = authSettings["Issuer"];
var audience = authSettings["Audience"];

builder.Services.AddAuthentication("Bearer") // <== NOTE : chaîne explicite
    .AddJwtBearer("Bearer", options => // <== NOTE : nom explicite à nouveau
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("AUTH FAILED: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("AUTH SUCCESS: " + context.SecurityToken);
                return Task.CompletedTask;
            }
        };
    });

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine("---- GATEWAY - HEADERS REÇUS ----");
    foreach (var header in context.Request.Headers)
    {
        Console.WriteLine($"{header.Key}: {header.Value}");
    }
    Console.WriteLine("----------------------------------");

    await next.Invoke();
});


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();


app.Run();
