using System.IO.Ports;
using backend.Models;
using backend.Repositories;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public class SerialPortBackgroundService : BackgroundService
{
    private readonly ILogger<SerialPortBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private SerialPort _serialPort;

    
    public SerialPortBackgroundService(
        ILogger<SerialPortBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SerialPortBackgroundService starting...");
        
        // Configure serial port (adjust these values to match your Arduino settings)
        _serialPort = new SerialPort
        {
            
            PortName = "COM8", // Change to your Arduino's COM port
            BaudRate = 9600,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            ReadTimeout = 500,
            WriteTimeout = 500
        };

        try
        {
            _serialPort.Open();
            _logger.LogInformation($"Serial port {_serialPort.PortName} opened");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        string message = _serialPort.ReadLine();
                        _logger.LogInformation($"Received: {message}");
                        await ProcessMessage(message);
                    }
                }
                catch (TimeoutException) { /* Normal during waiting period */ }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading from serial port");
                }

                await Task.Delay(100, stoppingToken);
            }
        }
        finally
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                _logger.LogInformation("Serial port closed");
            }
        }
    }

    private async Task ProcessMessage(string message)
    {
        try
        {
            // Example message format: "TEMP:25.5,MOISTURE:65,MOTION:1"
            var parts = message.Split(',');
            var values = new Dictionary<string, string>();
            
            foreach (var part in parts)
            {
                var keyValue = part.Split(':');
                if (keyValue.Length == 2)
                {
                    values[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
            
            await repository.DeleteAllAsync();

            var reading = new SensorReading
            {
                Temperature = values.TryGetValue("TEMP", out var temp) ? float.Parse(temp, CultureInfo.InvariantCulture) : null,
                MoistureLevel = values.TryGetValue("MOISTURE", out var moisture) ? int.Parse(moisture) : null,
                MotionDetected = values.TryGetValue("MOTION", out var motion) ? motion == "1" : null,
                Timestamp = DateTime.UtcNow
            };

            
            await repository.AddAsync(reading);
            _logger.LogInformation("Saved sensor reading to database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }
    }
}