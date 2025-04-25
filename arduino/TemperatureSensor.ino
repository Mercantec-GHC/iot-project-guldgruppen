void getTemperaturSensorRead() {
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