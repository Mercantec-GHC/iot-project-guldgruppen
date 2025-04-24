//TEST BITCHES!!!
void setup() {
  // put your setup code here, to run once:
    Serial.begin(9600);
    while(!Serial);
    carrier.noCase();
    carrier.begin();
    delay(2000);

}

void loop() {
  // put your main code here, to run repeatedly:
  float temperature = carrier.Env.readTemperature();
    Serial.print("Temperatur: ");
    Serial.print(temperature);
    Serial.println(" °C");

     delay(5000);
}
