using Microsoft.EntityFrameworkCore;
using ModulBank.Application.Interfaces;
using ModulBank.Application.Services;
using ModulBank.DataAccess;
using ModulBank.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// настройка PostgreSQL
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// регистрация репозиториев
builder.Services.AddScoped<IGameRepository, GameRepository>();

// регистрация сервисов
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// проверка на существование БД
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
