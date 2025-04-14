using MicroFrontEnd.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"ENVIRONNEMENT CHARGÉ : {builder.Environment.EnvironmentName}");
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var jwtSection = builder.Configuration.GetSection("AuthSettings");
builder.Services.Configure<AuthSettings>(jwtSection);
var authSettings = jwtSection.Get<AuthSettings>();
var secretKey = authSettings?.SecretKey;
var issuer = authSettings?.Issuer;
var audience = authSettings?.Audience;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("[AUTH FAIL] Erreur : " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("[AUTH SUCCESS] Token validé pour : " + context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient("LoggedClient");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();


app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    var method = context.Request.Method.ToUpperInvariant();
    var token = context.Session.GetString("JwtToken");

    // On autorise les chemins suivants même sans token
    var isAllowedPath =
        path.StartsWith("/auth") ||
        path.StartsWith("/css") ||
        path.StartsWith("/js") ||
        path.StartsWith("/lib") ||
        path.StartsWith("/images") ||
        path.StartsWith("/favicon");

    var isGetRequest = method == "GET";

    if (string.IsNullOrEmpty(token) && !isAllowedPath && isGetRequest)
    {
        Console.WriteLine($"[AUTH MIDDLEWARE] Redirection vers /Auth/Login – Path bloqué : {path}");
        context.Response.Redirect("/Auth/Login");
        return;
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();