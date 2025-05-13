#include <WiFiNINA.h>
#include <WiFiServer.h>
#include <FlashStorage.h>
#include <ArduinoHttpClient.h>
#include <Arduino_MKRIoTCarrier.h>

MKRIoTCarrier carrier; 

#define USE_HARDCODED_WIFI true
const char* HARDCODED_SSID = "iPhone";
const char* HARDCODED_PASS = "mysamus123";

WiFiServer server(80);

typedef struct {
  char ssid[32];
  char pass[64];
} WiFiCredentials;

FlashStorage(wifiCredsStore, WiFiCredentials);

char serverAddress[] = "172.20.10.2";
int serverPort = 5001;
const char* arduinoId = "123e4567-e89b-12d3-a456-426614174000";

WiFiClient wifi;
HttpClient client(wifi, serverAddress, serverPort);

unsigned long lastPostTime = 0;
const unsigned long postInterval = 1000; // 5 seconds

const int MOISTURE_PIN = A1;
const int PIR_PIN = A5;

void setup() {
  Serial.begin(9600);
  while (!Serial); // Wait for serial connection
  
  carrier.noCase();
  carrier.begin();

  if (!carrier.Env.begin()) {
    Serial.println("Failed to initialize ENV sensor!");
    while (1);
  }
  pinMode(PIR_PIN, INPUT);

  // Connect to WiFi
  bool connected = false;
  
  if (USE_HARDCODED_WIFI) {
    Serial.print("Connecting to WiFi...");
    WiFi.begin(HARDCODED_SSID, HARDCODED_PASS);
    
    for (int i = 0; i < 10; i++) {
      if (WiFi.status() == WL_CONNECTED) {
        connected = true;
        break;
      }
      delay(1000);
      Serial.print(".");
    }
  }

  if (!connected) {
    WiFiCredentials creds = wifiCredsStore.read();
    if (strlen(creds.ssid) > 0) {
      Serial.print("Trying saved WiFi...");
      WiFi.begin(creds.ssid, creds.pass);
      
      for (int i = 0; i < 10; i++) {
        if (WiFi.status() == WL_CONNECTED) {
          connected = true;
          break;
        }
        delay(1000);
        Serial.print(".");
      }
    }
  }

  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("\nConnected!");
    Serial.print("IP address: ");
    Serial.println(WiFi.localIP());
  } else {
    Serial.println("\nStarting AP mode");
    WiFi.beginAP("ArduinoSetup");
    server.begin();
  }
}

void loop() {
  carrier.Buttons.update();

  if (WiFi.status() == WL_AP_LISTENING || WiFi.status() == WL_AP_CONNECTED) {
    handleSetupPortal();
    return; // Skip main logic in AP mode
  }

  if (WiFi.status() != WL_CONNECTED) {
    delay(5000);
    return;
  }

  if (carrier.Buttons.onTouchDown(TOUCH0)) {
    sendSensorReadingRequest(arduinoId);
  }

  if (millis() - lastPostTime >= postInterval) {
    float temperature = carrier.Env.readTemperature();
    bool motion = digitalRead(PIR_PIN) == HIGH;
    int moisture = map(analogRead(MOISTURE_PIN), 1023, 0, 0, 100);
    moisture = constrain(moisture, 0, 100);
    
    sendSensorData(temperature, motion, moisture);
    lastPostTime = millis();
  }
}

void sendSensorData(float temp, bool motion, int moisture) {
  Serial.println("Preparing to send data...");
  
  String data = "{\"arduinoId\":\"" + String(arduinoId) + "\",";
  data += "\"temperature\":" + String(temp, 2) + ",";
  data += "\"motionDetected\":" + String(motion ? "true" : "false") + ",";
  data += "\"moistureLevel\":" + String(moisture) + "}";

  Serial.println("Sending: " + data);
  
  client.beginRequest();
  client.post("/api/sensor/reading");
  client.sendHeader("Content-Type", "application/json");
  client.sendHeader("Content-Length", data.length());
  client.beginBody();
  client.print(data);
  client.endRequest();

  int status = client.responseStatusCode();
  String response = client.responseBody();
  
  Serial.print("Status: ");
  Serial.println(status);
  Serial.print("Response: ");
  Serial.println(response);
  
  client.stop();
}

void sendSensorReadingRequest(const String& id) {
  Serial.println("Sending email request...");
  
  String data = "\"" + id + "\"";
  
  client.beginRequest();
  client.post("/Mail/send-sensor-reading");
  client.sendHeader("Content-Type", "application/json");
  client.sendHeader("Content-Length", data.length());
  client.beginBody();
  client.print(data);
  client.endRequest();

  int status = client.responseStatusCode();
  String response = client.responseBody();
  
  Serial.print("Email status: ");
  Serial.println(status);
  Serial.print("Response: ");
  Serial.println(response);
  
  client.stop();
}

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
        ssid.toCharArray(creds.ssid, 32);
        pass.toCharArray(creds.pass, 64);
        wifiCredsStore.write(creds);
        
        client.println("HTTP/1.1 200 OK");
        client.println("Content-Type: text/html");
        client.println();
        client.println("<h1>Saved. Rebooting...</h1>");
        delay(2000);
        NVIC_SystemReset();
      }
    } else {
      client.println("HTTP/1.1 200 OK");
      client.println("Content-Type: text/html");
      client.println();
      client.println("<h1>WiFi Setup</h1>");
      client.println("<form action='/save' method='GET'>");
      client.println("SSID: <input name='ssid'><br>");
      client.println("Password: <input name='pass' type='password'><br>");
      client.println("<input type='submit' value='Save'>");
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