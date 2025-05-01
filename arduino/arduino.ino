#include <WiFiNINA.h>
#include <ArduinoHttpClient.h>
#include "TemperatureSensor.h"
#include "PIRSensor.h"
#include "MoistureSensor.h"

// Wi-Fi credentials
char ssid[] = "Zyxel_BA2F";
char pass[] = "G7QLB4EAMY";

// Server settings
char serverAddress[] = "176.9.37.136";
int serverPort = 5000;

WiFiClient wifi;
HttpClient client = HttpClient(wifi, serverAddress, serverPort);

// Timing
unsigned long lastPostTime = 0;
const unsigned long postInterval = 1000;

void setup() {
  Serial.begin(9600);
  
  // Connect to Wi-Fi
  while (WiFi.begin(ssid, pass) != WL_CONNECTED) {
    Serial.println("Connecting to WiFi...");
    delay(1000);
  }
  Serial.println("Connected to WiFi!");

  setupTemperatureSensor();
  setupPIRSensor();
  setupMoistureSensor();
}

void loop() {
  float temperature = readTemperature();
  bool motionDetected = readMotion();
  int moisture = readMoistureLevel();

  unsigned long currentTime = millis();
  if (currentTime - lastPostTime >= postInterval) {
    sendSensorData(temperature, motionDetected, moisture);
    lastPostTime = currentTime;
  }
}

void sendSensorData(float temperature, bool motionDetected, int moisture) {
  String postData = "{\"temperature\":" + String(temperature, 2) +
                    ",\"motionDetected\":" + String(motionDetected ? "true" : "false") +
                    ",\"moistureLevel\":" + String(moisture) + "}";

  client.beginRequest();
  client.post("/api/sensor/reading");
  client.sendHeader("Content-Type", "application/json");
  client.sendHeader("Content-Length", postData.length());
  client.beginBody();
  client.print(postData);
  client.endRequest();

  int statusCode = client.responseStatusCode();
  String response = client.responseBody();
  
  Serial.print("Status code: ");
  Serial.println(statusCode);
  Serial.print("Response: ");
  Serial.println(response);
}
