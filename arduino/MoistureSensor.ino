const int moistureSensorPin = A0;

void getMoistureSensorRead() {
  int sensorValue = analogRead(moistureSensorPin);
  
  int moisturePercentage = map(sensorValue, 0, 1023, 0, 100);
  
  Serial.print(",MOISTURE:");
  Serial.print(moisturePercentage);
  //Serial.println("%");
}