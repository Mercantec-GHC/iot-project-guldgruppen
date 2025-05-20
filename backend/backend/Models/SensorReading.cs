namespace backend.Models;

// Model for sensoraflæsninger - gemmes i databasen
public class SensorReading
{
    public int Id { get; set; }
    public string ArduinoId { get; set; }
    public float? Temperature { get; set; }
    public bool? MotionDetected { get; set; }
    public float? HumidityLevel { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Data Transfer Object (DTO) for temperaturmålinger
public class TemperatureReadingDto
{
    public string ArduinoId { get; set; }
    public float Temperature { get; set; }
}

// Data Transfer Object (DTO) for bevægelsesmålinger
public class MotionReadingDto
{
    public string ArduinoId { get; set; }
    public bool MotionDetected { get; set; }
}

// Data Transfer Object (DTO) for fugtighedsmålinger
public class HumidityReadingDto
{
    public string ArduinoId { get; set; }
    public float HumidityLevel { get; set; }
}

// Kombineret DTO der kan indeholde alle målingstyper
public class CombinedReadingDto
{
    public string ArduinoId { get; set; }
    public float? Temperature { get; set; }
    public bool MotionDetected { get; set; }
    public float? HumidityLevel { get; set; }
}