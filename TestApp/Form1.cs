using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using waspmoteInterpreter;
using Serial;
using System.IO;

namespace WaspmoteGasSensorReagingGUI
{
    public partial class WaspGUI : Form
    {
        SerialPortManager spManager;
        StreamWriter sw1;
        StreamWriter sw2;
        StreamWriter sw3;
        Reading sensorReading;
        Interpreter interpreter;

        public WaspGUI()
        {
            InitializeComponent();
            UserInitialization();
        }

        private void UserInitialization()
        {
            interpreter = new Interpreter();
            sw1 = new StreamWriter("readings.out");
            sw2 = new StreamWriter("formatReadings.out");
            sw3 = new StreamWriter("exceptionLog.out");
            spManager = new SerialPortManager();
            SerialSettings mySerialSettings = spManager.CurrentSerialSettings;
            serialSettingsBindingSource.DataSource = mySerialSettings;
            portName.DataSource = mySerialSettings.PortNameCollection;
            baudRate.DataSource = mySerialSettings.BaudRateCollection;
           
            spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(spManager_NewSerialDataRecieved);
            this.FormClosing += new FormClosingEventHandler(WaspGUI_FormClosing);
        }

        void spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {

            if (this.InvokeRequired)
            {
                // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.
                this.BeginInvoke(new EventHandler<SerialDataEventArgs>(spManager_NewSerialDataRecieved), new object[] { sender, e });
                return;
            }

            int maxTextLength = 1000; // maximum text length in text box
            if (readings.TextLength > maxTextLength)
                readings.Text = readings.Text.Remove(0, readings.TextLength - maxTextLength);

            string str = Encoding.ASCII.GetString(e.Data);

            try
            {
                interpreter.ParseInput(str);
            }
            catch (Exception ex)
            {
                sw3.WriteLine("Exception Message: " + ex.Message);
                sw3.WriteLine("Exception Source: " + ex.Source);
                sw3.WriteLine("Exception InnerException: " + ex.InnerException);
                sw3.WriteLine("Exception TargetSite: " + ex.TargetSite);
                sw3.WriteLine("#######################################");
            }
            
            if (sensorReading != interpreter.CurrentReading)
            {
                sensorReading = interpreter.CurrentReading;
                UpdateManometers();
                readings.AppendText("\r\n" + str);
                readings.ScrollToCaret();

                string s = string.Format("TEMP:{0} | HUM:{1} | CO:{2} | CO2:{3}",
                sensorReading.Temperature, sensorReading.Humidity, sensorReading.COLevel, sensorReading.CO2Level);

                formattedReadings.AppendText("\r\n" + s);
                formattedReadings.ScrollToCaret();

                sw2.WriteLine(s);


            }

            sw1.WriteLine(str);

        }

        private void UpdateManometers()
        {
            if (sensorReading != null)
            {
                thermometer.Value = sensorReading.Temperature;
                hygrometer.Value = sensorReading.Humidity;
                coMeter.Value = sensorReading.COLevel;
                co2Meter.Value = sensorReading.CO2Level;
            }
        }

        private void WaspGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (spManager != null)
            {
                spManager.StopListening();
                spManager.Dispose();
            }

            sw1.Close();
            sw2.Close();
            sw3.Close();
        }
     
        private void StartListen_Click(object sender, EventArgs e)
        {
            spManager.StartListening();
        }

        private void StopListen_Click(object sender, EventArgs e)
        {
            spManager.StopListening();
        }
        
    }
}
