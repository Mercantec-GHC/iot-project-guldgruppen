using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using backend.Repositories;
using backend.Services;
using System.Text;
using backend.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Konfigurer server URLs - lyt på alle netværksinterfaces på port 5001
builder.WebHost.UseUrls("http://0.0.0.0:5001");

/*** SERVICE KONFIGURATION ***/

// Tilføj databasekontekst med PostgreSQL provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Tilføj repositories og services med dependency injection
builder.Services.AddScoped<ISensorRepository, SensorRepository>(); // Sensor repository
builder.Services.AddScoped<JwtTokenService>(); // JWT token service

// Tilføj MVC controllers
builder.Services.AddControllers();

// Konfigurer mailindstillinger fra appsettings.json
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IMailService, MailService>(); // Mail service med transient lifetime


/*** AUTENTIFIKATION KONFIGURATION ***/
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Valider token signatur
            IssuerSigningKey = new SymmetricSecurityKey( // Hent signaturnøgle fra konfig
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"])),
            ValidateIssuer = false, // Ikke valider udsteder (issuer)
            ValidateAudience = false, // Ikke valider modtager (audience)
            ValidateLifetime = true, // Valider token udløbstid
            ClockSkew = TimeSpan.Zero // Ingen tolerance for klokkeslæt
        };
    });


/*** CORS KONFIGURATION ***/
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => 
        builder.WithOrigins("http://localhost:5173", "http://176.9.37.136:5173") // Tilladte origins
            .AllowAnyMethod() // Tillad alle HTTP-metoder
            .AllowAnyHeader() // Tillad alle headers
            .AllowCredentials()); // Tillad credentials (cookies, auth headers)
});

// Tilføj Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*** APP OPBYGNING ***/
var app = builder.Build();

// Aktiver Swagger middleware (både UI og JSON endpoint)
app.UseSwagger();
app.UseSwaggerUI();

/*** MIDDLEWARE PIPELINE ***/
// Brugerdefineret logging middleware
app.Use(async (context, next) =>
{
    var remoteIp = context.Connection.RemoteIpAddress;
    Console.WriteLine($"Incoming request from {remoteIp}: {context.Request.Method} {context.Request.Path}");
    await next.Invoke();
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});

// HTTPS omdirigering (kun i produktion)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Standard ASP.NET Core middleware i korrekt rækkefølge
app.UseRouting(); // Routing middleware
app.UseCors("AllowAll"); // CORS policy
app.UseAuthentication(); // Autentifikation
app.UseAuthorization(); // Autorisation
app.MapControllers(); // Controller endpoints

// Start applikationen
app.Run();