using collegeSchedule.API.Data;
using collegeSchedule.API.Middlewares;
using collegeSchedule.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

//Формирование строки подключения из переменных
var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};"
                     + $"Port={Environment.GetEnvironmentVariable("DB_PORT")};"
                     + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
                     + $"Username={Environment.GetEnvironmentVariable("DB_USER")};"
                     + $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IScheduleService, ScheduleService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();
app.UseRouting(); // Важно для маршрутизации контроллеров
app.MapControllers(); // Карта контроллеров

app.Run();
