using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace waspmoteInterpreter
{

    public class Interpreter
    {
        State state;
        Reading current;

        public Reading CurrentReading
        {
            get { return current; }
            set { current = value; }
        }
        float val;
        int pos;
        int pow;
        FloatState fSt;

        float temp;
        float hum;
        float coVal;
        float co2Val;

        public Interpreter()
        {
            pos = 0;
            Init();
        }

        private void Init()
        {
            state = State.Head1;
            val = 0f;
            pow = 1;
            fSt = FloatState.Integer;

            temp = 0f;
            hum = 0f;
            coVal = 0f;
            co2Val = 0f;
        }

        public void ParseInput(string buffer)
        {
            pos = 0;

            switch (state)
            {
                case State.Head1:
                    S(buffer);
                    break;
                case State.Head2:
                    Head(buffer);
                    break;
                case State.Temp:
                    Temperature(buffer);
                    break;
                case State.Hum:
                    Humidity(buffer);
                    break;
                case State.CO:
                    CO(buffer);
                    break;
                case State.CO2:
                    CO2(buffer);
                    break;
            }
        }

        #region Recursive-Descent

     /*
     * S    -> STRING "#" HEAD
     * HEAD -> STRING "#" TEMP
     * TEMP -> FLOAT "#" HUM
     * HUM  -> FLOAT "#" CO
     * CO   -> FLOAT "#" CO2
     * CO2  -> FLOAT "#"
     * FLOAT-> INT "." INT
     * INT  -> [0..9]+
     * STRING: Any char combination except "#"     
     */

        private void S(string buffer)
        {
            Init();

            while (buffer[pos] != '#')
            {
                pos++;
                if (pos == buffer.Length)
                    return;
            }

            pos++;
            Head(buffer);
        }

        private void Head(string buffer)
        {
            state = State.Head2;

            while (buffer[pos] != '#')
            {
                pos++;
                if (pos == buffer.Length)
                    return;
            }

            pos++;
            Temperature(buffer);
        }

        private void Temperature(string buffer)
        {
            state = State.Temp;

            bool done = ParseFloat(buffer);

            if (done)
            {
                fSt = FloatState.Integer;
                temp = val;
                val = 0f;
                pow = 1;

                pos++;
                Humidity(buffer);
            }
        }

        private void Humidity(string buffer)
        {
            state = State.Hum;

            bool done = ParseFloat(buffer);

            if (done)
            {
                fSt = FloatState.Integer;
                hum = val;
                val = 0f;
                pow = 1;

                pos++;
                CO(buffer);
            }
        }

        private void CO(string buffer)
        {
            state = State.CO;

            bool done = ParseFloat(buffer);

            if (done)
            {
                fSt = FloatState.Integer;
                coVal = val;
                val = 0f;
                pow = 1;

                pos++;
                CO2(buffer);
            }
        }

        private void CO2(string buffer)
        {
            state = State.CO2;

            bool done = ParseFloat(buffer);

            if (done)
            {
                fSt = FloatState.Integer;
                co2Val = val;
                val = 0f;
                pow = 1;

                current = new Reading(temp, hum, coVal, co2Val);
                pos++;
                S(buffer);
            }
        }


        private bool ParseFloat(string buffer)
        {
            if (pos == buffer.Length)
                return false;

            //In case the next string begins with # ending the previous data string
            if (buffer[pos] == '#')
            {
                pos++;
                return true;
            }

            bool neg = false;

            if (buffer[pos] == '-')
            {
                neg = true;
                pos++;
            }

            if (fSt == FloatState.Integer)
            {
                while (buffer[pos] != '.')
                {
                    val = val * 10 + int.Parse(buffer[pos].ToString());

                    pos++;
                    if (pos == buffer.Length)
                        return false;
                }
            }

            fSt = FloatState.Decimal;

            pos++;
            if (pos == buffer.Length)
                return false;

            while (buffer[pos] != '#')
            {
                val = val + (float)(int.Parse(buffer[pos].ToString()) * Math.Pow(10, -pow));

                pow++;

                pos++;
                if (pos == buffer.Length)
                    return false;
            }

            if (neg)
                val *= -1;

            return true;
        }

        #endregion

    }

    enum State
    {
        Head1,
        Head2,
        Temp,
        Hum,
        CO,
        CO2
    }

    enum FloatState {Integer, Decimal }
}
