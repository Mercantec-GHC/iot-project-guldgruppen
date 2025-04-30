const int PIR_PIN = A5;

void setupPIRSensor() {
  pinMode(PIR_PIN, INPUT);
}

bool readMotion() {
  return digitalRead(PIR_PIN) == HIGH;
}
