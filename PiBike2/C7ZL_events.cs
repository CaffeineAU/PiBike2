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
        public event EventHandler ButtonUpPressed;
        public event EventHandler ButtonUpReleased;
        public event EventHandler ButtonDownPressed;
        public event EventHandler ButtonDownReleased;
        public event EventHandler ButtonEnterPressed;
        public event EventHandler ButtonEnterReleased;
        public event EventHandler DialClickedClockwise;
        public event EventHandler DialClickedAnticlockwise;

        public event EventHandler HeartRateChanged;
        public event EventHandler RpmChanged;


        private DateTime m_last_hr = DateTime.Now;
        private DateTime m_last_cadence = DateTime.Now;


        private void M_wheel_3_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                Debug.WriteLine("Button Released");
                if (ButtonEnterReleased != null)
                {
                    ButtonEnterReleased(this, null);
                }
            }
            else
            {
                Debug.WriteLine("Button Pressed");
                if (ButtonEnterPressed != null)
                {
                    ButtonEnterPressed(this, null);
                }
            }
        }



        private void M_button_up_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                Debug.WriteLine("Button Up Released");
                if (ButtonUpReleased != null)
                {
                    ButtonUpReleased(this, null);
                }
            }
            else
            {
                Debug.WriteLine("Button Up Pressed");
                if (ButtonUpPressed != null)
                {
                    ButtonUpPressed(this, null);
                }
            }
            
        }

        private void M_button_down_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                Debug.WriteLine("Button Down Released");
                if (ButtonDownReleased != null)
                {
                    ButtonDownReleased(this, null);
                }
            }
            else
            {
                Debug.WriteLine("Button Down Pressed");
                if (ButtonDownPressed != null)
                {
                    ButtonDownPressed(this, null);
                }
            }
        }


        private void M_heart_rate_sensor_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
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

        }



        private void M_cadence_sensor_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
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


        private int[] buff = new int[5];

        private void DialPollCallback(object state)
        {
            //Debug.WriteLine("CheckDial");

            int val = toInt(m_wheel_1.Read()) + (toInt(m_wheel_2.Read()) * 2);

            if (buff[4] != val)
            {
                buff[0] = buff[1];
                buff[1] = buff[2];
                buff[2] = buff[3];
                buff[3] = buff[4];
                buff[4] = val;

                if (buff[0] == 3 &&
                    buff[1] == 1 &&
                    buff[2] == 0 &&
                    buff[3] == 2 &&
                    buff[4] == 3)
                {
                    Debug.WriteLine("Dial AntiClockwise");
                    if(DialClickedAnticlockwise != null)
                    {
                        DialClickedAnticlockwise(this, null);
                    }
                }
                else if (buff[0] == 3 &&
                    buff[1] == 2 &&
                    buff[2] == 0 &&
                    buff[3] == 1 &&
                    buff[4] == 3)
                {
                    Debug.WriteLine("Dial Clockwise");
                    if (DialClickedClockwise != null)
                    {
                        DialClickedClockwise(this, null);
                    }
                }

            }
        }

        }
    }
