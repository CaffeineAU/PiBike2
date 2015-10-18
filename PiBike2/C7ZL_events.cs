using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace PiBike2
{
    partial class C7ZL
    {
        public event EventHandler YellowButtonPressed;
        public event EventHandler GreenButtonPressed;
        public event EventHandler HeartRateChanged;
        public event EventHandler RpmChanged;

        private DateTime m_last_hr = DateTime.Now;
        private DateTime m_last_cadence = DateTime.Now;



        private void M_pin_yellow_button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (YellowButtonPressed != null)
            {
                YellowButtonPressed(this, null);
                ToggleLEDSTest();
            }
        }

        private void M_pin_green_button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (GreenButtonPressed != null)
            {
                GreenButtonPressed(this, null);
                ToggleLEDSTest();
            }
        }

        private void ToggleLEDSTest()
        {
            if (m_pin_red_led.Read() == GpioPinValue.Low)
            {
                m_pin_red_led.Write(GpioPinValue.High);
            }
            else
            {
                m_pin_red_led.Write(GpioPinValue.Low);
            }

            if (m_pin_blue_led.Read() == GpioPinValue.Low)
            {
                m_pin_blue_led.Write(GpioPinValue.High);
            }
            else
            {
                m_pin_blue_led.Write(GpioPinValue.Low);
            }


        }

        

        private void M_pin_heart_rate_sensor_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            TimeSpan hr_interval = DateTime.Now - m_last_hr;

            m_last_hr = DateTime.Now;

            HeartRate = (int)Math.Floor(60 / hr_interval.TotalSeconds);

            if (HeartRateChanged != null)
            {
                HeartRateChanged(this, null);
            }

            //toggle the red LED
            if (m_pin_red_led.Read() == GpioPinValue.Low)
            {
                m_pin_red_led.Write(GpioPinValue.High);
            }
            else
            {
                m_pin_red_led.Write(GpioPinValue.Low);
            }

        }

        

        private void M_pin_cadence_sensor_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            TimeSpan cadence_interval = DateTime.Now - m_last_cadence;

            m_last_cadence = DateTime.Now;

            RPM = (int)Math.Floor(60 / cadence_interval.TotalSeconds);

            if (RpmChanged != null)
            {
                RpmChanged(this, null);
            }
        }
    }
}
