using IdentityAuthService.Models;
using IdentityAuthService.Service;
using IdentityAuthService.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration binding
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("AuthSettings"));
builder.Services.Configure<AdminCredentials>(builder.Configuration.GetSection("AdminCredentials"));
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// Important pour que les routes [Route(...)] fonctionnent
app.UseRouting();

app.Use(async (context, next) =>
{
    context.Request.Scheme = "http"; // Facultatif, tu peux probablement le supprimer
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Authz doit venir après Routing
app.UseAuthorization();

// Active les endpoints des controllers
app.MapControllers();

app.Run();
