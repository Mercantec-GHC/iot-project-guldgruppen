#include <WiFiNINA.h>
#include <WiFiServer.h>
#include <FlashStorage.h>
#include <ArduinoHttpClient.h>
#include <Arduino_MKRIoTCarrier.h>
 
MKRIoTCarrier carrier;
 
// === Configuration ===
#define USE_HARDCODED_WIFI true
const char* HARDCODED_SSID = "NOKIA-9791";
const char* HARDCODED_PASS = "ET6YVtZN4U";
char serverAddress[] = "192.168.0.250";
int serverPort = 5001;
const char* arduinoId = "123e4567-e89b-12d3-a456-426614174003";
const unsigned long postInterval = 1000;  // 1 second
 
WiFiServer server(80);
WiFiClient wifi;
HttpClient client(wifi, serverAddress, serverPort);
unsigned long lastPostTime = 0;
 
typedef struct {
  char ssid[32];
  char pass[64];
} WiFiCredentials;
 
FlashStorage(wifiCredsStore, WiFiCredentials);
 
// === Setup ===
void setup() {
  Serial.begin(9600);
  carrier.noCase();
  carrier.begin();
  delay(2000);
 
  pinMode(A2, INPUT);  // PIR sensor
  // Moisture sensor on A1 (default analog read — no setup needed)
 
  bool connected = false;
 
  if (USE_HARDCODED_WIFI) {
    Serial.print("Connecting with hardcoded WiFi: ");
    Serial.println(HARDCODED_SSID);
    WiFi.begin(HARDCODED_SSID, HARDCODED_PASS);
 
    for (int i = 0; i < 10 && WiFi.status() != WL_CONNECTED; i++) {
      delay(1000);
      Serial.print(".");
    }
 
    if (WiFi.status() == WL_CONNECTED) {
      Serial.println("\nConnected to WiFi with hardcoded credentials!");
      connected = true;
    } else {
      Serial.println("\nFailed to connect with hardcoded credentials.");
    }
  }
 
  if (!connected) {
    WiFiCredentials creds = wifiCredsStore.read();
    if (strlen(creds.ssid) > 0 && strlen(creds.pass) > 0) {
      WiFi.begin(creds.ssid, creds.pass);
 
      for (int i = 0; i < 10 && WiFi.status() != WL_CONNECTED; i++) {
        delay(1000);
        Serial.print(".");
      }
 
      if (WiFi.status() == WL_CONNECTED) {
        Serial.println("\nConnected to WiFi using saved credentials!");
        connected = true;
      } else {
        Serial.println("\nFailed to connect using saved credentials.");
      }
    }
  }
 
  if (!connected) {
    Serial.println("Starting AP for setup...");
    WiFi.beginAP("Arduino-Setup");
    Serial.print("AP IP Address: ");
    Serial.println(WiFi.localIP());
    server.begin();
  }
}
 
// === Main Loop ===
void loop() {
  if (WiFi.status() == WL_AP_LISTENING || WiFi.status() == WL_AP_CONNECTED) {
    handleSetupPortal();
  } else {
    runMainLogic();
  }
}
 
// === Web Setup Portal ===
void handleSetupPortal() {
  WiFiClient client = server.available();
  if (client) {
    String request = client.readStringUntil('\r');
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
        NVIC_SystemReset();
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
 
// === Sensor Logic ===
void runMainLogic() {
  float temperature = carrier.Env.readTemperature();  // replaced ENV with carrier.Env
  bool motionDetected = digitalRead(A2) == HIGH;
  int moisture = analogRead(A1);
 
  unsigned long currentTime = millis();
  if (currentTime - lastPostTime >= postInterval) {
    sendSensorData(temperature, motionDetected, moisture);
    lastPostTime = currentTime;
  }
}
 
// === HTTP POST ===
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