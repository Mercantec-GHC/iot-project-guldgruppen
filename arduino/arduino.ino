#include <WiFiNINA.h>
#include <ArduinoHttpClient.h>
#include <Arduino_MKRIoTCarrier.h>

MKRIoTCarrier carrier;

char ssid[] = "Zyxel_BA2F";
char pass[] = "G7QLB4EAMY";

char serverAddress[] = "176.9.37.136";
int serverPort = 5000;

WiFiClient wifi;
HttpClient client = HttpClient(wifi, serverAddress, serverPort);

unsigned long lastPostTime = 0;
const unsigned long postInterval = 1000;

const int PIR_PIN = A5;

const int MOISTURE_PIN = A1;

const int correctPIN[3] = {1, 3, 2}; // A=0, B=2, C=1

bool requestingPIN = false;
int pinIndex = 0;
int enteredPIN[3];

void setupTemperatureSensor() {
  if (!carrier.Env.begin()) {
    Serial.println("Failed to initialize ENV sensor!");
    while (1);
  }
}

void setupPIRSensor() {
  pinMode(PIR_PIN, INPUT);
}

void setupMoistureSensor() {
}

void setup() {
  Serial.begin(9600);
  CARRIER_CASE = false;
  carrier.begin();
  carrier.display.setRotation(0);
  carrier.display.fillScreen(ST77XX_BLACK);
  carrier.display.setTextColor(ST77XX_WHITE);
  carrier.display.setTextSize(2);
  
  while (WiFi.begin(ssid, pass) != WL_CONNECTED) {
    Serial.println("Connecting to WiFi...");
    delay(1000);
  }
  Serial.println("Connected to WiFi!");

  setupTemperatureSensor();
  setupPIRSensor();
  setupMoistureSensor();
}

float readTemperature() {
  return carrier.Env.readTemperature();
}

bool readMotion() {
  return digitalRead(PIR_PIN) == HIGH;
}

int readMoistureLevel() {
  int sensorValue = analogRead(MOISTURE_PIN);
  int moisturePercent = map(sensorValue, 1023, 0, 0, 100);
  return constrain(moisturePercent, 0, 100);
}

void showMessage(String msg) {
  carrier.display.fillScreen(ST77XX_BLACK);
  carrier.display.setCursor(20, 60);
  carrier.display.print(msg);
}

bool isCorrectPIN() {
  for (int i = 0; i < 3; i++) {
    if (enteredPIN[i] != correctPIN[i]) return false;
  }
  return true;
}

void loop() {
  float temperature = readTemperature();
  bool motionDetected = readMotion();
  int moisture = readMoistureLevel();

  if (motionDetected && !requestingPIN) {
    requestingPIN = true;
    pinIndex = 0;
    showMessage("Enter PIN");
  }

  if (requestingPIN) {
    carrier.Buttons.update();

    if (carrier.Buttons.onTouchDown(TOUCH0)) { // Button A = 0
      enteredPIN[pinIndex++] = 1;
    } else if (carrier.Buttons.onTouchDown(TOUCH1)) { // Button B = 1
      enteredPIN[pinIndex++] = 2;
    } else if (carrier.Buttons.onTouchDown(TOUCH2)) { // Button C = 2
      enteredPIN[pinIndex++] = 3;
    }

    if (pinIndex == 3) {
      if (isCorrectPIN()) {
        showMessage("Temp: " + String(temperature, 1) + "C\nMoist: " + String(moisture) + "%");
      } else {
        showMessage("Wrong password");
      }
      delay(3000);
      requestingPIN = false;
      carrier.display.fillScreen(ST77XX_BLACK);
    }

    delay(100);
  }

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
