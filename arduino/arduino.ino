//TEST BITCHES!!!

void setup() {
  // This code runs only once when the Arduino is turned on or reset

  Serial.begin(9600); // Start the communication with the computer (Serial Monitor) on "baud rate = 9600".
  while (!Serial);    // Wait until the Serial Monitor is ready

  carrier.noCase();   // Disable button sensitivity on the Carrier (not using buttons here)
  carrier.begin();    //  initializes the Carrier board (gets it ready). activates sensors, display, buttons, etc.

  delay(2000);        // Wait for 2 seconds (gives sensors time to get ready)
}

void loop() {
  // This part runs over and over as long as the Arduino is powered on.


  float temperature = carrier.Env.readTemperature();
     /* This reads the temperature from the environmental sensor on the Carrier.
     The value is stored in a variable called temperature.
     It’s a float. */


  // Print the temperature reading to the Serial Monitor
  Serial.print("Temperatur: ");  // Print text ("Temperature")
  Serial.print(temperature);     // Print the actual temperature value
  Serial.println(" °C");         // Print the unit (Celsius) and move to the next line

  delay(5000); // Wait 5 seconds before measuring again (so it doesn't spam output)
}
