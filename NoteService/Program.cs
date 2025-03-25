using NoteService.Repositories;
using NoteService.Repositories.Interface;
using NoteService.Services;
using NoteService.Services.Interface;
using NoteService.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<NoteDatabaseSettings>(
    builder.Configuration.GetSection("NoteDatabaseSettings"));

builder.Services.AddSingleton<INoteRepository, NoteRepository>();
builder.Services.AddScoped<INoteService, NotesService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
