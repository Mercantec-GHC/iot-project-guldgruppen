using backend.Repositories;
using backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container BEFORE calling builder.Build()

// Add background service
builder.Services.AddHostedService<SerialPortBackgroundService>();

// Add EF Core Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repository
builder.Services.AddScoped<ISensorRepository, SensorRepository>();

// Add controllers
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); // After this point, you can't add services anymore

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enhanced request logging
app.Use(async (context, next) =>
{
    var remoteIp = context.Connection.RemoteIpAddress;
    Console.WriteLine($"Incoming request from {remoteIp}: {context.Request.Method} {context.Request.Path}");
    await next.Invoke();
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});

app.UseHttpsRedirection();
app.UseCors(); // Use the default policy we configured earlier
app.UseAuthorization();
app.MapControllers();

app.Run();