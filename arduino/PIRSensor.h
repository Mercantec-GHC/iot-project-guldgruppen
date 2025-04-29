const int PIR_PIN = 2; // Connect your PIR sensor to digital pin 2

void setupPIRSensor() {
  pinMode(PIR_PIN, INPUT);
}

bool readMotion() {
  return digitalRead(PIR_PIN) == HIGH;
}
