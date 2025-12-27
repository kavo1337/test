using Microsoft.EntityFrameworkCore;
using MVVM.Api.Data;
using MVVM.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    if (!dbContext.TodoItems.Any())
    {
        dbContext.TodoItems.AddRange(
            new TodoItem { Title = "Изучить основы MVVM", IsDone = false },
            new TodoItem { Title = "Вызвать API из WPF", IsDone = false },
            new TodoItem { Title = "Сохранить данные в SQL Server", IsDone = true }
        );
        dbContext.SaveChanges();
    }
}

app.Run();
