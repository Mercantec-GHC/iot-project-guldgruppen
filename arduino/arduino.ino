#include <WiFiNINA.h>
#include <WiFiServer.h>
#include <FlashStorage.h>
#include <ArduinoHttpClient.h>

#include "TemperatureSensor.h"
#include "MoistureSensor.h"
#include "PIRSensor.h"

WiFiServer server(80);

// Struct to store WiFi credentials
typedef struct {
  char ssid[32];
  char pass[64];
} WiFiCredentials;

FlashStorage(wifiCredsStore, WiFiCredentials);

// Server settings
char serverAddress[] = "192.168.1.234";
int serverPort = 5001;

// Unique Arduino GUID
const char* arduinoId = "123e4567-e89b-12d3-a456-426614174001";

WiFiClient wifi;
HttpClient client(wifi, serverAddress, serverPort);

// Timing
unsigned long lastPostTime = 0;
const unsigned long postInterval = 1000;

void setup() {
  Serial.begin(9600);
  delay(2000); // Wait for Serial monitor

  setupTemperatureSensor();
  setupMoistureSensor();
  setupPIRSensor();

  WiFiCredentials creds = wifiCredsStore.read();

  if (strlen(creds.ssid) == 0 || strlen(creds.pass) == 0) {
    Serial.println("No saved WiFi. Starting AP setup...");
    WiFi.beginAP("Arduino-Setup");
    Serial.print("AP IP Address: "); Serial.println(WiFi.localIP());
    server.begin();
  } else {
    Serial.print("Connecting to WiFi: "); Serial.println(creds.ssid);
    WiFi.begin(creds.ssid, creds.pass);

    int attempt = 0;
    while (WiFi.status() != WL_CONNECTED && attempt < 10) {
      delay(1000);
      Serial.print(".");
      attempt++;
    }

    if (WiFi.status() == WL_CONNECTED) {
      Serial.println("\nConnected to WiFi!");
    } else {
      Serial.println("\nFailed to connect. Starting AP fallback...");
      WiFi.beginAP("Arduino-Setup");
      Serial.print("AP IP Address: "); Serial.println(WiFi.localIP());
      server.begin();
    }
  }
}

void loop() {
  if (WiFi.status() == WL_AP_LISTENING || WiFi.status() == WL_AP_CONNECTED) {
    handleSetupPortal();
  } else {
    runMainLogic();
  }
}

void handleSetupPortal() {
  WiFiClient client = server.available();
  if (client) {
    Serial.println("Client connected for setup");
    String request = client.readStringUntil('\r');
    Serial.println(request);
    client.flush();

    if (request.indexOf("/save?") != -1) {
      String ssid = getParam(request, "ssid");
      String pass = getParam(request, "pass");

      if (ssid.length() > 0 && pass.length() > 0) {
        WiFiCredentials creds;
        ssid.toCharArray(creds.ssid, sizeof(creds.ssid));
        pass.toCharArray(creds.pass, sizeof(creds.pass));
        wifiCredsStore.write(creds);

        client.println("HTTP/1.1 200 OK");
        client.println("Content-Type: text/html");
        client.println();
        client.println("<h1>Saved! Rebooting...</h1>");
        delay(2000);
        NVIC_SystemReset(); // Soft reset
      }
    } else {
      client.println("HTTP/1.1 200 OK");
      client.println("Content-Type: text/html");
      client.println();
      client.println("<h1>WiFi Setup</h1>");
      client.println("<form action=\"/save\" method=\"GET\">");
      client.println("SSID: <input name=\"ssid\"><br>");
      client.println("Password: <input name=\"pass\" type=\"password\"><br>");
      client.println("<input type=\"submit\" value=\"Save\">");
      client.println("</form>");
    }
    client.stop();
  }
}

String getParam(String request, String key) {
  int start = request.indexOf(key + "=");
  if (start == -1) return "";
  start += key.length() + 1;
  int end = request.indexOf('&', start);
  if (end == -1) end = request.indexOf(' ', start);
  return request.substring(start, end);
}

void runMainLogic() {
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
  String postData = "{\"arduinoId\":\"" + String(arduinoId) + "\"," +
                    "\"temperature\":" + String(temperature, 2) +
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
