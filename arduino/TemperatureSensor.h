#include <Arduino_MKRENV.h>

void setupTemperatureSensor() {
  if (!ENV.begin()) {
    Serial.println("Failed to initialize ENV sensor!");
    while (1);
  }
}

float readTemperature() {
  return ENV.readTemperature();
}