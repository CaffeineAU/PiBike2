using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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


        private Queue<DateTime> m_cadence_blips = new Queue<DateTime>();

        private Timer cadence_timeout;


        private void ClearRPM()
        {
            this.RPM = 0;

            m_cadence_blips.Clear();

            Debug.WriteLine("Clear RPM");
        }

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
                Debug.WriteLine("{0} Heart beat", DateTime.Now);

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
                Debug.WriteLine("{0} Pedal Sensor", DateTime.Now);

                //TimeSpan cadence_interval = DateTime.Now - m_last_cadence;

                //m_last_cadence = DateTime.Now;


                m_cadence_blips.Enqueue(DateTime.Now);

                if(m_cadence_blips.Count > 5)
                {
                    m_cadence_blips.Dequeue();
                }

                if (m_cadence_blips.Count > 2)
                {

                    double cadence_interval = (m_cadence_blips.Last() - m_cadence_blips.First()).TotalSeconds / m_cadence_blips.Count;

                    Debug.WriteLine("Interval = {0} (count = {1})", cadence_interval, m_cadence_blips.Count);


                    RPM = (int)Math.Floor(30 / cadence_interval); //its 60 divided by 2 due to the 2 magnets
                }


               
            }
        }


        private int[] buff = new int[5];

        private TimeSpan rpm_timeout = new TimeSpan(0, 0, 5);

        private void DialPollCallback(object state)
        {
            //Debug.WriteLine("CheckDial");

            if (m_cadence_blips.Count > 0)
            {
                if (DateTime.Now - m_cadence_blips.Last() > rpm_timeout)
                {
                    ClearRPM();
                }
            }

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
