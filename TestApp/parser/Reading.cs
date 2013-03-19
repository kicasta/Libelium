using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace waspmoteInterpreter
{
    public class Reading
    {
        float humidity;

        public float Humidity
        {
            get { return humidity; }
            set { humidity = value; }
        }
        float temperature;

        public float Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }
        float coLevel;

        public float COLevel
        {
            get { return coLevel; }
            set { coLevel = value; }
        }
        float co2Level;

        public float CO2Level
        {
            get { return co2Level; }
            set { co2Level = value; }
        }
        float battery;

        public Reading(float temperature, float humidity, float coLevel, float co2Level)
        {
            this.humidity = humidity;
            this.temperature = temperature;
            this.coLevel = coLevel;
            this.co2Level = co2Level;
           
        }

        public Reading() : this(0, 0, 0, 0) { }

    }
}
