const int MOISTURE_PIN = A1;

void setupMoistureSensor(){}

int carrier.Env.readMoistureLevel() {
  int sensorValue = analogRead(MOISTURE_PIN);
  int moisturePercent = map(sensorValue, 1023, 0, 0, 100);
  moisturePercent = constrain(moisturePercent, 0, 100);
  return moisturePercent;
}