using backend.Models;
using Microsoft.EntityFrameworkCore;

// Hoved-DbContext klasse der repræsenterer databasen og dens tabeller
public class AppDbContext : DbContext
{
    // Constructor der modtager konfigurationsindstillinger for DbContext
    // options: Indstillinger for databasetilkobling og konfiguration
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // DbSet der repræsenterer Users-tabellen i databasen
    // Giver adgang til CRUD-operationer for brugere
    public DbSet<User> Users { get; set; }
    
    // DbSet der repræsenterer SensorReadings-tabellen i databasen
    // Giver adgang til CRUD-operationer for sensoraflæsninger
    public DbSet<SensorReading> SensorReadings { get; set; }
}