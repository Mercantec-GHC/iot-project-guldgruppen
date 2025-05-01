namespace backend.Models;

public class SensorReading
{
    public int Id { get; set; }
    
    public string ArduinoId { get; set; }
    public float? Temperature { get; set; }
    public bool? MotionDetected { get; set; }
    public int? MoistureLevel { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class TemperatureReadingDto
{
    public string ArduinoId { get; set; }
    public float Temperature { get; set; }
}

public class MotionReadingDto
{
    public string ArduinoId { get; set; }
    public bool MotionDetected { get; set; }
}

public class MoistureReadingDto
{
    public string ArduinoId { get; set; }
    public int MoistureLevel { get; set; }
}

public class CombinedReadingDto
{
    public string ArduinoId { get; set; }
    public float Temperature { get; set; }
    public bool MotionDetected { get; set; }
    public int MoistureLevel { get; set; }
}