#include <WiFiNINA.h>
#include <WiFiServer.h>
#include <FlashStorage.h>
#include <ArduinoHttpClient.h>
#include <Arduino_MKRENV.h>

#define USE_HARDCODED_WIFI true
const char* HARDCODED_SSID = "Zyxel_BA2F";
const char* HARDCODED_PASS = "G7QLB4EAMY";

WiFiServer server(80);

// Struct til at gemme WiFi loginoplysninger
typedef struct {
  char ssid[32];
  char pass[64];
} WiFiCredentials;

// Flash-lager til at gemme WiFi credentials
FlashStorage(wifiCredsStore, WiFiCredentials);

// Server indstillinger
char serverAddress[] = "192.168.1.234";
int serverPort = 5001;

// Unikt ID for Arduino'en
const char* arduinoId = "123e4567-e89b-12d3-a456-426614174001";

WiFiClient wifi;
HttpClient client(wifi, serverAddress, serverPort);

// Timing variabler
unsigned long lastPostTime = 0;
const unsigned long postInterval = 1000;

// Fugtighedssensor pin
const int MOISTURE_PIN = A1;

// Bevægelsessensor (PIR) pin
const int PIR_PIN = A5;

// Opsætning af fugtighedssensor (tom - kun til fremtidig brug)
void setupMoistureSensor(){}

// Læser fugtighedsniveau fra sensoren og returnerer som procent
int readMoistureLevel() {
  int sensorValue = analogRead(MOISTURE_PIN);
  int moisturePercent = map(sensorValue, 1023, 0, 0, 100);
  moisturePercent = constrain(moisturePercent, 0, 100);
  return moisturePercent;
}

// Opsætning af PIR (bevægelses)sensor
void setupPIRSensor() {
  pinMode(PIR_PIN, INPUT);
}

// Læser bevægelsessensoren og returnerer true hvis der er bevægelse
bool readMotion() {
  return digitalRead(PIR_PIN) == HIGH;
}

// Opsætning af temperatur sensor (fra MKR ENV shield)
void setupTemperatureSensor() {
  if (!ENV.begin()) {
    Serial.println("Failed to initialize ENV sensor!");
    while (1);
  }
}

// Læser temperatur fra sensoren
float readTemperature() {
  return ENV.readTemperature();
}

// Hovedopsætningsfunktion - kører én gang ved start
void setup() {
  Serial.begin(9600);
  delay(2000); // Vent på Serial monitor

  setupTemperatureSensor();
  setupMoistureSensor();
  setupPIRSensor();

  bool connected = false;

  // Prøv at forbinde med hardcodede WiFi credentials
  if (USE_HARDCODED_WIFI) {
    Serial.print("Connecting with hardcoded WiFi: ");
    Serial.println(HARDCODED_SSID);
    WiFi.begin(HARDCODED_SSID, HARDCODED_PASS);

    int attempt = 0;
    while (WiFi.status() != WL_CONNECTED && attempt < 10) {
      delay(1000);
      Serial.print(".");
      attempt++;
    }

    if (WiFi.status() == WL_CONNECTED) {
      Serial.println("\nConnected to WiFi with hardcoded credentials!");
      connected = true;
    } else {
      Serial.println("\nFailed to connect with hardcoded credentials.");
    }
  }

  // Hvis ikke forbundet, prøv gemte credentials fra flash
  if (!connected) {
    WiFiCredentials creds = wifiCredsStore.read();

    if (strlen(creds.ssid) > 0 && strlen(creds.pass) > 0) {
      Serial.print("Connecting to saved WiFi: ");
      Serial.println(creds.ssid);
      WiFi.begin(creds.ssid, creds.pass);

      int attempt = 0;
      while (WiFi.status() != WL_CONNECTED && attempt < 10) {
        delay(1000);
        Serial.print(".");
        attempt++;
      }

      if (WiFi.status() == WL_CONNECTED) {
        Serial.println("\nConnected to WiFi using saved credentials!");
        connected = true;
      } else {
        Serial.println("\nFailed to connect using saved credentials.");
      }
    }
  }

  // Hvis stadig ikke forbundet, start som access point til opsætning
  if (!connected) {
    Serial.println("Starting AP for setup...");
    WiFi.beginAP("Arduino-Setup");
    Serial.print("AP IP Address: ");
    Serial.println(WiFi.localIP());
    server.begin();
  }
}

// Hovedloop - kører kontinuerligt
void loop() {
  // Hvis i opsætningsmode, håndter setup portal
  if (WiFi.status() == WL_AP_LISTENING || WiFi.status() == WL_AP_CONNECTED) {
    handleSetupPortal();
  } else {
    // Ellers kør normal logik
    runMainLogic();
  }
}

// Håndterer WiFi opsætningsportalen
void handleSetupPortal() {
  WiFiClient client = server.available();
  if (client) {
    Serial.println("Client connected for setup");
    String request = client.readStringUntil('\r');
    Serial.println(request);
    client.flush();

    // Håndterer /save request med nye WiFi credentials
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
      // Viser WiFi opsætningsformular
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

// Hjælpefunktion til at udtrække parametre fra HTTP request
String getParam(String request, String key) {
  int start = request.indexOf(key + "=");
  if (start == -1) return "";
  start += key.length() + 1;
  int end = request.indexOf('&', start);
  if (end == -1) end = request.indexOf(' ', start);
  return request.substring(start, end);
}

// Hovedlogik - læser sensorer og sender data
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

// Sender sensordata til serveren via HTTP POST
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