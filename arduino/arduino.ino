#include <Arduino_MKRIoTCarrier.h>

MKRIoTCarrier carrier;

int pir = A5;
bool screenActive = false;
unsigned long screenOnTime = 0;  // Time when screen was activated
const unsigned long screenDuration = 30000; // 30 seconds

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
  getPIRSensorRead();
  getMoistureSensorRead();
  getTemperaturSensorRead();
}
