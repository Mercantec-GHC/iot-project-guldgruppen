﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250520171106_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("backend.Models.SensorReading", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ArduinoId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float?>("HumidityLevel")
                        .HasColumnType("real");

                    b.Property<bool?>("MotionDetected")
                        .HasColumnType("boolean");

                    b.Property<float?>("Temperature")
                        .HasColumnType("real");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("SensorReadings");
                });

            modelBuilder.Entity("backend.Models.User", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<string>("ArduinoId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float?>("HumidityThreshold")
                        .HasColumnType("real");

                    b.Property<DateTime?>("LastHumidityEmailSentAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastHumiditySmsSentAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastMotionAlertSentAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastTemperatureEmailSentAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastTemperatureSmsSentAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("SendEmailAlert")
                        .HasColumnType("boolean");

                    b.Property<bool>("SendHumidityAlert")
                        .HasColumnType("boolean");

                    b.Property<bool>("SendTemperatureAlert")
                        .HasColumnType("boolean");

                    b.Property<float?>("TemperatureThreshold")
                        .HasColumnType("real");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
