#include <Arduino_MKRIoTCarrier.h>

MKRIoTCarrier carrier;

int pir = A5;
bool alarmEnabled = true;

void setup() {

  Serial.begin(9600);
  while (!Serial);  // venter på at serial monitor åbner

  carrier.noCase(); // Ingen plastik case
  carrier.begin();  // initialiserer carrier sensors

  // Sæt PIR sensor som input
  pinMode(pir, INPUT);

   // Display rotation
  carrier.display.setRotation(0); // Rotation 0 eller 1,2,3 (90, 180, 270 grader)

  // Baggrundsfarve
  carrier.display.fillScreen(0x001F);

  //tekst farve
  carrier.display.setTextColor(0xFFFF);

  // tekst størrelse
  carrier.display.setTextSize(2);

  // Position af tekst
  carrier.display.setCursor(30, 100);

  // Print tekst på display
  carrier.display.print("Guldgruppen!");

}

void loop() {
 
  //Pir sensor
  int motion = digitalRead(pir);
  Serial.print("PIR Sensor: ");
  Serial.println(motion == HIGH && alarmEnabled ? "MOTION DETECTED!" : "No motion");

   delay(500);

}
