#include <WiFi.h>
#include <PubSubClient.h>
#include <WiFiClientSecure.h>
#include <string>
#include <Adafruit_NeoPixel.h>
Adafruit_NeoPixel ledRGB(1, 48, NEO_GRB + NEO_KHZ800);

// WiFi
const char *ssid = "example";          // Название точки доступа
const char *password = "example";   // Пароль

// MQTT Broker
const char *mqtt_broker = "example.ru";
const char *mqtt_username = "example";
const char *mqtt_password = "example";
const int mqtt_port = 1883;

// Инициализация WiFi и MQTT
WiFiClientSecure esp_client;
PubSubClient mqtt_client(esp_client);

// TLS сертификат
const char* ca_cert= \
"-----BEGIN CERTIFICATE-----\n" \
"-----END CERTIFICATE-----\n";

void connectToWiFi();

void connectToMQTT();

void mqttCallback(char *topic, byte *payload, unsigned int length);


void setup() {
    Serial.begin(115200);

    // Работа со светодиодом
    ledRGB.begin();

    // Подключение к WiFi
    connectToWiFi();

    // Применение TLS сертификата
    esp_client.setCACert(ca_cert);

    // Подключение к MQTT
    mqtt_client.setServer(mqtt_broker, mqtt_port);
    mqtt_client.setKeepAlive(60);
    mqtt_client.setCallback(mqttCallback);
    connectToMQTT();
}

void connectToWiFi() {
    WiFi.begin(ssid, password);
    Serial.print("Подключение к WiFi");
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }
    Serial.println("\nПодключен к WiFi");
}

void connectToMQTT() {
    while (!mqtt_client.connected()) {
        String client_id = "esp32-client-" + String(WiFi.macAddress());
        Serial.printf("Подключение к MQTT как %s...\n", client_id.c_str());
        if (mqtt_client.connect(client_id.c_str(), mqtt_username, mqtt_password)) {
            Serial.println("Подключен к MQTT");
            
            // Подписка на темы
            mqtt_client.subscribe("light");
            mqtt_client.subscribe("brightness");
            mqtt_client.subscribe("temperature");

        } else {
            Serial.print("Не удалось подключиться к MQTT, rc=");
            Serial.print(mqtt_client.state());
            Serial.println(" Повторная попытка через 5 секунд.");
            delay(5000);
        }
    }
}

String readMessage(byte *payload, unsigned int length) {
  String message;
    for (unsigned int i = 0; i < length; i++) {
        message += (char) payload[i];
    }

  return message;
}

bool isLightOn;

void mqttCallback(char *topic, byte *payload, unsigned int length) {
    Serial.print("Получено сообщение на тему: ");
    Serial.println(topic);

    Serial.print("Сообщение: ");
    
    String message = readMessage(payload, length);
    Serial.println(message);

    // Включение светодиода
    if(strcmp(topic, "light") == 0) {
      if(message == "0") {
        ledRGB.fill(0x000000);
        ledRGB.show();
        isLightOn = false;
      }

      if(message == "1") {
        ledRGB.fill(0xFF0000);
        ledRGB.show();
        isLightOn = true;
      }
    }

    // Яркость светодиода
    if(strcmp(topic, "brightness") == 0) {
      // Переводим сообщение из строки в число
      int value = message.toInt();

      ledRGB.setBrightness(value);
      
      if(isLightOn) {
        ledRGB.fill(0xFF0000);
        ledRGB.show();
      }
    }
}

unsigned long timing; // Переменная для хранения точки отсчета
long randNumber; // Случайное число

void loop() {
    if (!mqtt_client.connected()) {
        connectToMQTT();
    }
    mqtt_client.loop();


  // Отправка случайного числа на тему "temperature" для теста графика температуры
  if (millis() - timing > 3000){ // Вместо 10000 подставьте нужное вам значение паузы 
    timing = millis(); 
    randNumber = random(20, 30);

    if (mqtt_client.connected()) {
        // Преобразование числа в строку
        std::string str = std::to_string(randNumber);

        // Отправка
        mqtt_client.publish("temperature", str.c_str());
    }
  }
}