using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddingTempAndMoistAlert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastHumidityAlertSentAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTemperatureAlertSentAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HumidityThreshold",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendHumidityAlert",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SendTemperatureAlert",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "TemperatureThreshold",
                table: "Users",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastHumidityAlertSentAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastTemperatureAlertSentAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HumidityThreshold",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SendHumidityAlert",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SendTemperatureAlert",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TemperatureThreshold",
                table: "Users");
        }
    }
}
