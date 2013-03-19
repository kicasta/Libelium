//#include <WaspSensorGas_v20.h>

#define GAIN  1  //GAIN of the sensor stage CO2
#define RESISTOR 100  // LOAD RESISTOR of the sensor stage


// variables
float temp = 0.0;
float hum = 0;
float coVal = 0.0;
float co2Val = 0.0;

char tempStr[10];
char humStr[10];
char coStr[10];
char co2Str[10];

//XBee 868 variables
packetXBee* paq_sent;
int8_t state=0;
long previous=0;
char data[50];

void setup()
{
  // 0. Init USB port for debugging
  USB.begin();
  //USB.println("USB port started...");
  
   // Inits the XBee 868 library
  xbee868.init(XBEE_868,FREQ868M,NORMAL);

  // Powers XBee
  xbee868.ON();
  
   // Turn on the sensor board
  SensorGasv20.setBoardMode(SENS_ON);
  
   // Configure the CO sensor 
  SensorGasv20.configureSensor(SENS_SOCKET4CO, GAIN, RESISTOR);
   
   // Configure the CO2 sensor socket
  SensorGasv20.configureSensor(SENS_CO2, GAIN);

  
  // Turn on the CO2 sensor and wait for stabilization and
  // sensor response time
  SensorGasv20.setSensorMode(SENS_ON, SENS_CO2);
  SensorGasv20.setSensorMode(SENS_ON, SENS_SOCKET4CO);
  
  
  delay(30000);
 
  RTC.ON();

}

void loop()
{
   
  //Measuring Sensors
  temp = SensorGasv20.readValue(SENS_TEMPERATURE);
  hum = SensorGasv20.readValue(SENS_HUMIDITY);
  coVal = SensorGasv20.readValue(SENS_SOCKET4CO);
  co2Val = SensorGasv20.readValue(SENS_CO2);
  
  //USB Print to Debug
  //USB.println(temp);
  //USB.println(hum);
  //USB.println(coVal);
  //USB.println(co2Val);
      
  //Convert readings  
  Utils.float2String(temp,tempStr,2);
  Utils.float2String(hum,humStr,2);
  Utils.float2String(coVal,coStr,2);
  Utils.float2String(co2Val,co2Str,2);
  
  //Copy to buffer
  sprintf(data,"#");
  sprintf(data + strlen(data),tempStr);
  sprintf(data + strlen(data),"#");
  sprintf(data + strlen(data),humStr);
  sprintf(data + strlen(data),"#");
  sprintf(data + strlen(data),coStr);
  sprintf(data + strlen(data),"#");
  sprintf(data + strlen(data),co2Str);
  sprintf(data + strlen(data),"#");

  //Set params to send
  paq_sent=(packetXBee*) calloc(1,sizeof(packetXBee));
  paq_sent->mode=UNICAST;
  paq_sent->MY_known=0;
  paq_sent->packetID=0x52;
  paq_sent->opt=0;
  xbee868.hops=0;
  xbee868.setOriginParams(paq_sent, "5678", MY_TYPE);
  xbee868.setDestinationParams(paq_sent, "0013A20040795858", data, MAC_TYPE, DATA_ABSOLUTE);
  xbee868.sendXBee(paq_sent);
  
  free(paq_sent);
  paq_sent=NULL;
  
  for(int i = 0; i < 50; i++)
  {
    data[i] = '\0';
  }
   
  delay(30000);  
}
