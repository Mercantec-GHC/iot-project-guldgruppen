#include <Arduino_MKRIoTCarrier.h>

MKRIoTCarrier carrier;

int pir = A5;
unsigned long lastSensorRead = 0;
const unsigned long sensorInterval = 1000; // 1 second

void setup() {
  Serial.begin(9600);
  while (!Serial);  // Wait for Serial Monitor

  carrier.noCase();
  carrier.begin();

  pinMode(pir, INPUT);

  carrier.display.setRotation(0);
  carrier.display.fillScreen(0x0000); // Start with screen off
}

void loop() {

  carrier.Buttons.update();
  unsigned long currentMillis = millis();

  getPIRSensorRead();  // Always check for PIR and button touches immediately

  if (currentMillis - lastSensorRead >= sensorInterval) {
    lastSensorRead = currentMillis;
    
    getTemperaturSensorRead();
    getMoistureSensorRead();
  }
}