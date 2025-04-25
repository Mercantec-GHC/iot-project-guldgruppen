bool screenActive = false;
unsigned long screenOnTime = 0;  // Time when screen was activated
const unsigned long screenDuration = 30000; // 30 seconds

void getPIRSensorRead() {
  int motion = digitalRead(pir);
  unsigned long currentTime = millis();

  Serial.print("PIR Sensor: ");
  Serial.println(motion == HIGH ? "MOTION DETECTED!" : "No motion");

  if (motion == HIGH ) {
    if (!screenActive) {
      // Turn on screen and ask for pin code
      carrier.display.fillScreen(0xFFFF); // White background
      carrier.display.setTextColor(0x0000); // Black text
      carrier.display.setTextSize(2);
      carrier.display.setCursor(30, 100);
      carrier.display.print("Pin code:");
      screenActive = true;
    }
    screenOnTime = currentTime; // Reset the 30-second timer
  }

  // Check if 30 seconds passed since last motion
  if (screenActive && (currentTime - screenOnTime > screenDuration)) {
    carrier.display.fillScreen(0x0000); // Turn off screen
    screenActive = false;
  }

  delay(100);
}