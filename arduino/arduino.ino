#include <Arduino_MKRIoTCarrier.h>

MKRIoTCarrier carrier;

// Sensor pins
const int pir = A5;
const int moistureSensorPin = A0;

// Timing variables
unsigned long previousMillis = 0;
const long interval = 5000; // 5 seconds in milliseconds

void setup() {
  Serial.begin(9600);
  while (!Serial); // Wait for serial connection
  
  carrier.noCase();
  carrier.begin();
  
  pinMode(pir, INPUT);
  pinMode(moistureSensorPin, INPUT);
  
  carrier.display.setRotation(0);
  carrier.display.fillScreen(0x0000); // Start with screen off
}

void loop() {
  unsigned long currentMillis = millis();
  
  // Check if 5 seconds have passed
  if (currentMillis - previousMillis >= interval) {
    previousMillis = currentMillis;
    
    // Read all sensors
    float temperature = carrier.Env.readTemperature();
    int moistureValue = analogRead(moistureSensorPin);
    int moisturePercentage = map(moistureValue, 0, 1023, 0, 100);
    int motion = digitalRead(pir);
    
    // Send data in simple key:value format
    Serial.print("TEMP:");
    Serial.print(temperature);
    Serial.print(",MOISTURE:");
    Serial.print(moisturePercentage);
    Serial.print(",MOTION:");
    Serial.println(motion == HIGH ? "1" : "0");
  }
  
  // Handle PIR sensor display separately
  handlePIRSensor();
}

void handlePIRSensor() {
  static bool screenActive = false;
  static unsigned long screenOnTime = 0;
  const unsigned long screenDuration = 30000; // 30 seconds

  int motion = digitalRead(pir);
  unsigned long currentTime = millis();

  if (motion == HIGH) {
    if (!screenActive) {
      // Turn on screen
      carrier.display.fillScreen(0xFFFF); // White background
      carrier.display.setTextColor(0x0000); // Black text
      carrier.display.setTextSize(2);
      carrier.display.setCursor(30, 100);
      carrier.display.print("Pin code:");
      screenActive = true;
    }
    screenOnTime = currentTime;
  }

  if (screenActive && (currentTime - screenOnTime > screenDuration)) {
    carrier.display.fillScreen(0x0000); // Turn off screen
    screenActive = false;
  }
}