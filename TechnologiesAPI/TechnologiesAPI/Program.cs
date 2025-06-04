using TechnologiesAPI;
using BotTG;
using Models;
using Data;
using Microsoft.EntityFrameworkCore;
using Data.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer("Data Source=WIN-57GSVVQFVLA;Initial Catalog=MyBotDB;Integrated Security=True;Pooling=True;Encrypt=False;Trust Server Certificate=True"));

builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ITechnologyRepository, TechnologyRepository>();
builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHostedService<TelegramBotService>();
builder.Services.AddScoped<TechnologyService>();


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
