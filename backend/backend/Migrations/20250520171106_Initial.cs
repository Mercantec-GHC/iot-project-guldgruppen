using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArduinoId = table.Column<string>(type: "text", nullable: false),
                    Temperature = table.Column<float>(type: "real", nullable: true),
                    MotionDetected = table.Column<bool>(type: "boolean", nullable: true),
                    HumidityLevel = table.Column<float>(type: "real", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordSalt = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    ArduinoId = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    SendEmailAlert = table.Column<bool>(type: "boolean", nullable: false),
                    LastMotionAlertSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TemperatureThreshold = table.Column<float>(type: "real", nullable: true),
                    SendTemperatureAlert = table.Column<bool>(type: "boolean", nullable: false),
                    HumidityThreshold = table.Column<float>(type: "real", nullable: true),
                    SendHumidityAlert = table.Column<bool>(type: "boolean", nullable: false),
                    LastTemperatureEmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastTemperatureSmsSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastHumidityEmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastHumiditySmsSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
