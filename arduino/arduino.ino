#include <WiFiNINA.h>
#include <WiFiServer.h>
#include <FlashStorage.h>
#include <ArduinoHttpClient.h>
#include <Arduino_MKRIoTCarrier.h>

MKRIoTCarrier carrier; // Carrier board objekt til sensorer og display

// WiFi indstillinger - kan bruge hardcodede eller gemte credentials
#define USE_HARDCODED_WIFI true
const char* HARDCODED_SSID = "iPhone";
const char* HARDCODED_PASS = "mysamus123";

WiFiServer server(80); // Opret en server på port 80

// Struct til at gemme WiFi loginoplysninger
typedef struct {
  char ssid[32];  // WiFi netværksnavn
  char pass[64];  // WiFi adgangskode
} WiFiCredentials;

// Flash-lager til at gemme WiFi credentials (beholder data ved genstart)
FlashStorage(wifiCredsStore, WiFiCredentials);

// Server indstillinger for backend kommunikation
char serverAddress[] = "172.20.10.2";
int serverPort = 5001;

// Unikt ID for Arduino'en (simulerer en UUID)
const char* arduinoId = "123e4567-e89b-12d3-a456-426614174000";

WiFiClient wifi; // WiFi klient objekt
HttpClient client(wifi, serverAddress, serverPort); // HTTP klient

// Timing variabler for regulær dataafsendelse
unsigned long lastPostTime = 0;
const unsigned long postInterval = 1000; // Interval i millisekunder (1 sekund)

// Sensor pins
const int MOISTURE_PIN = A1;  // Fugtighedssensor pin
const int PIR_PIN = A5;       // Bevægelsessensor (PIR) pin

void setup() {
  Serial.begin(9600);
  while (!Serial); // Vent på seriel forbindelse (kun nødvendigt ved debugging)
  
  // Initialiser carrier board
  carrier.noCase(); // Deaktiver beskyttelsescase (hvis bruges)
  carrier.begin();  // Start carrier board funktionalitet

  // Initialiser miljøsensor
  if (!carrier.Env.begin()) {
    Serial.println("Failed to initialize ENV sensor!");
    while (1); // Stop programmet hvis sensor ikke initialiseres
  }
  
  pinMode(PIR_PIN, INPUT); // Sæt PIR sensor pin som input

  // Forbind til WiFi
  bool connected = false;
  
  // Prøv først hardcodede WiFi oplysninger hvis aktiveret
  if (USE_HARDCODED_WIFI) {
    Serial.print("Connecting to WiFi...");
    WiFi.begin(HARDCODED_SSID, HARDCODED_PASS);
    
    // Prøv i 10 sekunder at forbinde
    for (int i = 0; i < 10; i++) {
      if (WiFi.status() == WL_CONNECTED) {
        connected = true;
        break;
      }
      delay(1000);
      Serial.print(".");
    }

      // Display rotation
  carrier.display.setRotation(0); // Rotation 0 eller 1,2,3 (90, 180, 270 grader)

  // Baggrundsfarve
  carrier.display.fillScreen(0x001F); 

  //tekst farve
  carrier.display.setTextColor(0xFFFF); 

  // tekst størrelse
  carrier.display.setTextSize(2); 
  
  // Position af tekst
  carrier.display.setCursor(20, 70);
  carrier.display.print("Climate Control");

  carrier.display.setCursor(50, 90);
  carrier.display.print("Sensor CPH");
  
  carrier.display.setCursor(20, 130);
  carrier.display.print("Send mail press 0");

  carrier.display.setCursor(20, 150);
  carrier.display.print("Show data press 1");

  
  }

  // Hvis ikke forbundet, prøv gemte credentials fra flash
  if (!connected) {
    WiFiCredentials creds = wifiCredsStore.read();
    if (strlen(creds.ssid) > 0) { // Tjek om der er gemte credentials
      Serial.print("Trying saved WiFi...");
      WiFi.begin(creds.ssid, creds.pass);
      
      // Prøv i 10 sekunder at forbinde
      for (int i = 0; i < 10; i++) {
        if (WiFi.status() == WL_CONNECTED) {
          connected = true;
          break;
        }
        delay(1000);
        Serial.print(".");
      }
    }
  }

  // Hvis forbundet, vis IP-adresse
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("\nConnected!");
    Serial.print("IP address: ");
    Serial.println(WiFi.localIP());
  } else {
    // Hvis ikke forbundet, start som access point til konfiguration
    Serial.println("\nStarting AP mode");
    WiFi.beginAP("ArduinoSetup"); // Opret WiFi netværk med navnet "ArduinoSetup"
    server.begin(); // Start webserveren
  }
}

void loop() {
  carrier.Buttons.update(); // Opdater knaptilstande

  // Hvis i AP mode, håndter konfigurationsportal
  if (WiFi.status() == WL_AP_LISTENING || WiFi.status() == WL_AP_CONNECTED) {
    handleSetupPortal();
    return; // Spring over hovedlogik i AP mode
  }

  // Hvis ikke forbundet til WiFi, vent og prøv igen
  if (WiFi.status() != WL_CONNECTED) {
    delay(5000);
    return;
  }

  // Hvis der trykkes på TOUCH0, send anmodning om sensorlæsning
  if (carrier.Buttons.onTouchDown(TOUCH0)) {
    sendSensorReadingRequest(arduinoId);

    //Display success tekst
    carrier.display.fillScreen(0x07E0); 
    carrier.display.setTextColor(0xFFFF); 
    carrier.display.setTextSize(2); 
    carrier.display.setCursor(50, 70);
    carrier.display.print("Mail Send");
    carrier.display.setCursor(50, 110);
    carrier.display.print("Successfully!");
    delay(2000);

    //Tilbage til Home screen
    carrier.display.fillScreen(0x001F);  
    carrier.display.setCursor(20, 70);
    carrier.display.print("Climate Control");

    carrier.display.setCursor(50, 90);
    carrier.display.print("Sensor CPH");
    
    carrier.display.setCursor(20, 130);
    carrier.display.print("Send mail press 0");

    carrier.display.setCursor(20, 150);
    carrier.display.print("Show data press 1");


  }

    // Hvis der trykkes på TOUCH1, vis temperatur og fugtighed
  if (carrier.Buttons.onTouchDown(TOUCH1)) {
    // Læs sensordata
    float temperature = carrier.Env.readTemperature();
    int moisture = map(analogRead(MOISTURE_PIN), 1023, 0, 0, 100);
    moisture = constrain(moisture, 0, 100);

    // Vis data på display
    carrier.display.fillScreen(0x0000);  
    carrier.display.setTextColor(0xFFFF); 
    carrier.display.setTextSize(2); 

    carrier.display.setCursor(30, 60);
    carrier.display.print("Temp: ");
    carrier.display.print(temperature);
    carrier.display.print(" C");

    carrier.display.setCursor(30, 100);
    carrier.display.print("Moisture: ");
    carrier.display.print(moisture);
    carrier.display.print(" %");

    delay(4000);  // Vis data i 4 sekunder

    // Tilbage til Home screen
    carrier.display.fillScreen(0x001F);  
    carrier.display.setCursor(20, 70);
    carrier.display.print("Climate Control");

    carrier.display.setCursor(50, 90);
    carrier.display.print("Sensor CPH");

    carrier.display.setCursor(20, 130);
    carrier.display.print("Send mail press 0");

    carrier.display.setCursor(20, 150);
    carrier.display.print("Show data press 1");
  }

  // Send sensordata med jævne mellemrum
  if (millis() - lastPostTime >= postInterval) {
    // Læs sensorværdier
    float temperature = carrier.Env.readTemperature(); // Temperatur fra carrier
    bool motion = digitalRead(PIR_PIN) == HIGH; // Bevægelsessensor (HIGH = bevægelse)
    // Læs fugtighed og konverter til procent (0-100)
    int moisture = map(analogRead(MOISTURE_PIN), 1023, 0, 0, 100);
    moisture = constrain(moisture, 0, 100); // Sikre værdi er mellem 0-100
    
    sendSensorData(temperature, motion, moisture); // Send data til server
    lastPostTime = millis(); // Opdater sidste sendetidspunkt
  }
}

// Funktion til at sende sensordata til serveren
void sendSensorData(float temp, bool motion, int moisture) {
  Serial.println("Preparing to send data...");
  
  // Opret JSON string med sensordata
  String data = "{\"arduinoId\":\"" + String(arduinoId) + "\",";
  data += "\"temperature\":" + String(temp, 2) + ","; // Temperatur med 2 decimaler
  data += "\"motionDetected\":" + String(motion ? "true" : "false") + ",";
  data += "\"moistureLevel\":" + String(moisture) + "}";

  Serial.println("Sending: " + data); // Debug output
  
  // Send HTTP POST request
  client.beginRequest();
  client.post("/api/sensor/reading"); // API endpoint
  client.sendHeader("Content-Type", "application/json");
  client.sendHeader("Content-Length", data.length());
  client.beginBody();
  client.print(data); // Send JSON data
  client.endRequest();

  // Læs svar fra serveren
  int status = client.responseStatusCode();
  String response = client.responseBody();
  
  // Udskriv svar til seriel monitor
  Serial.print("Status: ");
  Serial.println(status);
  Serial.print("Response: ");
  Serial.println(response);
  
  client.stop(); // Luk forbindelsen
}

// Funktion til at anmode om at få tilsendt sensordata på mail
void sendSensorReadingRequest(const String& id) {
  Serial.println("Sending email request...");
  
  String data = "\"" + id + "\""; // Send kun Arduino ID som data
  
  // Send HTTP POST request til mail-endpoint
  client.beginRequest();
  client.post("/Mail/send-sensor-reading");
  client.sendHeader("Content-Type", "application/json");
  client.sendHeader("Content-Length", data.length());
  client.beginBody();
  client.print(data);
  client.endRequest();

  // Læs svar fra serveren
  int status = client.responseStatusCode();
  String response = client.responseBody();
  
  // Udskriv svar til seriel monitor
  Serial.print("Email status: ");
  Serial.println(status);
  Serial.print("Response: ");
  Serial.println(response);
  
  client.stop(); // Luk forbindelsen
}

// Håndter WiFi konfigurationsportal når i AP mode
void handleSetupPortal() {
  WiFiClient client = server.available(); // Tjek for indkomne forbindelser
  if (client) {
    String request = client.readStringUntil('\r'); // Læs HTTP request
    client.flush();

    // Hvis request indeholder /save? (form submission)
    if (request.indexOf("/save?") != -1) {
      // Udtræk SSID og password fra URL parametre
      String ssid = getParam(request, "ssid");
      String pass = getParam(request, "pass");
      
      // Hvis både SSID og password er angivet, gem dem i flash
      if (ssid.length() > 0 && pass.length() > 0) {
        WiFiCredentials creds;
        ssid.toCharArray(creds.ssid, 32); // Kopier SSID til struct
        pass.toCharArray(creds.pass, 64); // Kopier password til struct
        wifiCredsStore.write(creds); // Gem i flash-lager
        
        // Send svar til browseren
        client.println("HTTP/1.1 200 OK");
        client.println("Content-Type: text/html");
        client.println();
        client.println("<h1>Saved. Rebooting...</h1>");
        delay(2000);
        NVIC_SystemReset(); // Genstart Arduino
      }
    } else {
      // Vis WiFi konfigurationsformular
      client.println("HTTP/1.1 200 OK");
      client.println("Content-Type: text/html");
      client.println();
      client.println("<h1>WiFi Setup</h1>");
      client.println("<form action='/save' method='GET'>");
      client.println("SSID: <input name='ssid'><br>");
      client.println("Password: <input name='pass' type='password'><br>");
      client.println("<input type='submit' value='Save'>");
      client.println("</form>");
    }
    client.stop(); // Luk forbindelsen
  }
}

// Hjælpefunktion til at udtrække parametre fra URL
String getParam(String request, String key) {
  int start = request.indexOf(key + "="); // Find start af parameter
  if (start == -1) return ""; // Hvis ikke fundet, returner tom streng
  start += key.length() + 1; // Flyt markør til efter '='
  int end = request.indexOf('&', start); // Find slutningen (enten & eller mellemrum)
  if (end == -1) end = request.indexOf(' ', start);
  return request.substring(start, end); // Returner parameter værdi
}