using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NoteService.Repositories;
using NoteService.Repositories.Interface;
using NoteService.Services;
using NoteService.Services.Interface;
using NoteService.Settings;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB settings
builder.Services.Configure<NoteDatabaseSettings>(
    builder.Configuration.GetSection("NoteDatabaseSettings"));

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var noteDbSettings = serviceProvider
        .GetRequiredService<IOptions<NoteDatabaseSettings>>()
        .Value;
    return new MongoClient(noteDbSettings.ConnectionString);
});

builder.Services.AddSingleton(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<NoteDatabaseSettings>>().Value;
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<INoteRepository, NoteRepository>();
builder.Services.AddScoped<INoteService, NotesService>();

// Auth settings
var jwtSettings = builder.Configuration.GetSection("AuthSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
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

        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();

app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    Console.WriteLine("[Middleware] Authorization header reçu : " + (authHeader ?? "AUCUN"));
    await next.Invoke();
});

app.UseAuthorization();

app.MapControllers();

app.Run();