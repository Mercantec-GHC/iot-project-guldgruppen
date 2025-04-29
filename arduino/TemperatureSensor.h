#include <Arduino_MKRENV.h> // Assuming you use the ENV shield for temperature sensor

void setupTemperatureSensor() {
  if (!ENV.begin()) {
    Serial.println("Failed to initialize ENV sensor!");
    while (1);
  }
}

float readTemperature() {
  return ENV.readTemperature();
}
