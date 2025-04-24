#include <Arduino_MKRIoTCarrier.h>
#include <WiFiNINA.h>
#include <PubSubClient.h> //Publish/Subscribe
#include "arduino_secrets.h"
 
MKRIoTCarrier carrier;
 
const char ssid[] = SECRET_SSID; //netværksnavn
const char password[] = SECRET_PASS;
 
//HQTT Broker info
const char mqttServer[] = SECRET_MQTTSERVER;
const int mqttPort = 8883;
const char mqttUser [] = "Mikkelkonyher";
const char mqttPassword [] = SECRET_MQTTPASS;
 
WiFiSSLClient wifiClient;
PubSubClient client(wifiClient);
 
int combinationStep = 0;
String alarmStatus = "Unknown";
 
// MQTT callback funktion
void mqttCallback(char* topic, byte* payload, unsigned int length) {
  String message;
  for (int i = 0; i < length; i++) {
    message += (char)payload[i];
  }
 
  if (message == "ALARM_TRIGGERED") {
    alarmStatus = "TRIGGERED";
    carrier.display.fillScreen(0xF800);
    carrier.display.setTextColor(0xFFFF);
    carrier.display.setCursor(30, 100);
    carrier.display.setRotation(0);
    carrier.display.setTextSize(2);
    carrier.display.print("ALARM!!");
  }
  else if (message == "ALARM_DISABLED") {
    alarmStatus = "DISABLED";
    carrier.display.fillScreen(0x07E0);
    carrier.display.setTextColor(0xFFFF);
    carrier.display.setCursor(30, 100);
    carrier.display.setRotation(0);
    carrier.display.setTextSize(2);
    carrier.display.print("Alarm OFF");
  }
}
 
// Funktion til at oprette forbindelse til MQTT
void connectToMQTT() {
  while (!client.connected()) {
    if (client.connect("RemoteControlArduino", mqttUser, mqttPassword)) {
      client.subscribe("guldgruppen/alarm");
    } else {
      delay(2000);
    }
  }
}
 
 
// Funktion der håndterer tryk på en bestemt knap som en del af en adgangskode-kombination.
void handleButtonPress(int button, int passwordOrder) {
  if (combinationStep == passwordOrder) {
    combinationStep++;
    Serial.print("Step ");
    Serial.print(passwordOrder);
    Serial.print(" correct (Button ");
    Serial.print(button);
    Serial.println(").");
  } else {
    // forkert knap brugt -> reset
    combinationStep = 0;
  }
}
 
///Disabler alarm og sender til MQTT
void disableAlarm() {
  
  // vis "Alarm Off" på skærmen
  carrier.leds.clear();
  carrier.leds.show();
  carrier.display.fillScreen(0xF81F);
  carrier.display.setTextSize(2);
  carrier.display.setCursor(40, 50);
  carrier.display.print("Alarm Off");
  client.publish("guldgruppen/alarm", "ALARM_DISABLED");

  combinationStep = 0;
}
 
void setup() {
  Serial.begin(9600);
  while (!Serial);
 
  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi...");
  int attempts = 0;
 
  //10 forsøg på at connnecte
  while (WiFi.status() != WL_CONNECTED && attempts < 10) {
    delay(1000);
    Serial.print(".");
    attempts++;
  }
 
    //Wifi status
    if (WiFi.status() == WL_CONNECTED) {
    Serial.println("Connected to WiFi!");
    Serial.print("IP address: ");
    Serial.println(WiFi.localIP());
  } else {
    Serial.println("Failed to connect to WiFi.");
    Serial.print("Wi-Fi Status: ");
    Serial.println(WiFi.status());
  }
 
  client.setServer(mqttServer, mqttPort); //angiv MQTT Server
  client.setCallback(mqttCallback);
  connectToMQTT();
 
  carrier.noCase();
  carrier.begin();
 
  carrier.display.fillScreen(0x07E0);
  carrier.display.setCursor(30, 100);
  carrier.display.setTextSize(2);
  carrier.display.setRotation(0);
  carrier.display.setTextColor(0xFFFF);
  carrier.display.print("Remote Control");
}
 
void loop() {

  //Sørger for at holde MQTT forbindelsen aktiv
  if (!client.connected()) {
    connectToMQTT();
  }
  client.loop();
 
  carrier.Buttons.update();
 
  // (kombination af tryk) Kombinations kode
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
    disableAlarm();
  }  
 
  // Armér alarm
  carrier.Buttons.update();
  if (carrier.Buttons.onTouchDown(TOUCH0)) {
    Serial.println("Alarm Armed");
    client.publish("guldgruppen/alarm", "ALARM_ARMED");
  }
}