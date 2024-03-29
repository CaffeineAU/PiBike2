﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace PiBike2
{
    partial class C7ZL
    {



        public C7ZL()
        {
            InitGPIO();
        }






        private int m_RPM;

        public int RPM
        {
            get { return m_RPM; }
            private set
            {
                m_RPM = value;

                if (RpmChanged != null)
                {
                    RpmChanged(this, null);
                }
            }
        }

        private int m_heart_rate;
 
        public int HeartRate
        {
            get { return m_heart_rate; }
            private set {
                m_heart_rate = value;

                
            }
        }


        private int m_difficulty_setpoint;

        public int Difficulty
        {
            get { return m_difficulty_setpoint; }
            set
            {
                if (value < 0)
                {
                    //throw new Exception("Difficulty too low");
                }
                else if (value > MAX_LEVEL)
                {
                    //throw new Exception("Difficulty too high");
                }
                else
                {
                    m_difficulty_setpoint = value;
                }
            }
        }

        public enum OnOff { On, Off};

        public OnOff iFitLED
        {
            get
            {
                if(m_ifit_led.Read() == GpioPinValue.High)
                {
                    return OnOff.On;
                }
                else
                {
                    return OnOff.Off;
                }
            }
            set
            {
                if(value == OnOff.On)
                {
                    m_ifit_led.Write(GpioPinValue.High);
                }
                else
                {
                    m_ifit_led.Write(GpioPinValue.Low);
                }
            }
        }

        

    }

    
}
