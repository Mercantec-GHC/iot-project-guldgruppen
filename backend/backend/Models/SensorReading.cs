namespace backend.Models;

public class SensorReading
{
    public int Id { get; set; }
    public float? Temperature { get; set; }
    public bool? MotionDetected { get; set; }
    public int? MoistureLevel { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}