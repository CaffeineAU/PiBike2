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



        private void M_pin_yellow_button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if ((args.Edge == GpioPinEdge.FallingEdge) && (YellowButtonPressed != null))
            {
                YellowButtonPressed(this, null);
            }
        }

        private void M_pin_green_button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if ((args.Edge == GpioPinEdge.FallingEdge) && (GreenButtonPressed != null))
            {
                GreenButtonPressed(this, null);
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
