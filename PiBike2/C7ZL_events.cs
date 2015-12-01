using System;
using System.Collections.Generic;
using System.Diagnostics;
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



        private void M_pin_wheel_1_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {

            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                if (m_pin_wheel_2.Read() == GpioPinValue.High)
                {
                    Debug.WriteLine("Left");
                }
                else
                {
                    Debug.WriteLine("Right");
                }
            }

        }

        private void M_pin_wheel_2_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
        }

        private void M_pin_wheel_3_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                Debug.WriteLine("Button Released");
            }
            else
            {
                Debug.WriteLine("Button Pressed");
            }
        }


        private void M_pin_heart_rate_sensor_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                TimeSpan hr_interval = DateTime.Now - m_last_hr;

                m_last_hr = DateTime.Now;

                HeartRate = (int)Math.Floor(60 / hr_interval.TotalSeconds);

                if (HeartRateChanged != null)
                {
                    HeartRateChanged(this, null);
                }
            }

            //toggle the red LED
            /*if (m_pin_red_led.Read() == GpioPinValue.Low)
            {
                m_pin_red_led.Write(GpioPinValue.High);
            }
            else
            {
                m_pin_red_led.Write(GpioPinValue.Low);
            }*/

        }



        private void M_pin_cadence_sensor_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
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
}
