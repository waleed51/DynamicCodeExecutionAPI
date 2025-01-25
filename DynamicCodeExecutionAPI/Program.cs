using DynamicCodeExecutionAPI.Contexts;
using DynamicCodeExecutionAPI.interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // we can use connection string in appsettings file to make it configurable.
    options.UseSqlite("Data Source=CodeEditor.db");
});
builder.Services.AddScoped<IExecuseCodeService, ExecuseCodeService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Allow the Angular app's origin
                  .AllowAnyHeader()
                  .AllowAnyMethod(); // Allow any HTTP method (GET, POST, etc.)
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");
app.UseAuthorization();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.EnsureCreated();

app.MapControllers();


app.Run();
