using Microsoft.EntityFrameworkCore;
using To_Do_List_backend.Services;
using To_Do_List_backend.Data;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<Context>(options => options.UseNpgsql(connection));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); //свагер

builder.Services.AddScoped<IToDoListService, ToDoListService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) //свагер
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors();

app.MapControllers();

app.Run();


/*using var serviceScope = app.Services.CreateScope();

var context = serviceScope.ServiceProvider.GetService<Context>();

context?.Database.Migrate();*/