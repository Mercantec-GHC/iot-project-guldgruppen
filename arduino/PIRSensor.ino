bool screenActive = false;
bool isAuthorized = false;
int combinationStep = 0;
float tempDisplay = 0.0;
int moistureDisplay = 0;



// Function handling buttons combination
void handleButtonPress(int button, int passwordOrder) {
  if (combinationStep == passwordOrder) {
    combinationStep++;
    Serial.print("Step ");
    Serial.print(passwordOrder);
    Serial.print(" correct (Button ");
    Serial.print(button);
    Serial.println(").");
  } else {
    // Wrong button resets combination
    combinationStep = 0;
  }
}

void DisplaySensorReadings() {
  

  // Read fresh sensor values
  tempDisplay = carrier.Env.readTemperature();
  
  int sensorValue = analogRead(moistureSensorPin);
  moistureDisplay = map(sensorValue, 0, 1023, 0, 100);

  // Clear and prepare the screen
  carrier.leds.clear();
  carrier.leds.show();
  carrier.display.fillScreen(0xF81F);
  carrier.display.setTextSize(2);

  // Print Temperature
  carrier.display.setCursor(20, 50);
  carrier.display.print("Temp: ");
  carrier.display.print(tempDisplay);
  carrier.display.println(" C");

  // Print Moisture
  carrier.display.setCursor(20, 100); // Move cursor down
  carrier.display.print("Moisture: ");
  carrier.display.print(moistureDisplay);
  carrier.display.println("%");

  delay(5000);
  carrier.display.fillScreen(0x0000); 
  screenActive = false;                            
  combinationStep = 0; 
}

void getPIRSensorRead() {
  int motion = digitalRead(pir);
  unsigned long currentTime = millis();

  // If motion is detected
  if (motion == HIGH && !screenActive) {
   
      // Turn on the screen and ask for PIN code
      carrier.display.fillScreen(0xFFFF); // White background
      carrier.display.setTextColor(0x0000); // Black text
      carrier.display.setTextSize(2);
      carrier.display.setCursor(30, 100);
      carrier.display.print("Pin code:");
      screenActive = true;  // Set screen to active
   }

  if (screenActive) {
    // PIN code entry and handling
    if (carrier.Buttons.onTouchDown(TOUCH1) && combinationStep == 0) {
      handleButtonPress(1, 0);
    }

    if (carrier.Buttons.onTouchDown(TOUCH3) && combinationStep == 1) {
      handleButtonPress(3, 1);
    }

    if (carrier.Buttons.onTouchDown(TOUCH2) && combinationStep == 2) {
      handleButtonPress(2, 2);
    }

    if (combinationStep == 3) {
      isAuthorized = true;
      DisplaySensorReadings();
     
    }
  }
}
