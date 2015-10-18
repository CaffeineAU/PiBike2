using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

namespace PiBike2
{
    partial class C7ZL
    {
        GpioController m_gpio;

        //Warning.  Microsoft reserved GPIO 17, 19, 20, and 21 for SPI1
        //Do NOT use for GPIO


        //Digital Outputs
        private GpioPin m_pin_blue_led = null;
        private const int BLUE_LED_PIN = 5;  //GPIO 5
        //RED LED
        private GpioPin m_pin_red_led = null;
        private const int RED_LED_PIN = 22;  //GPIO 22
        //Motor 1A, 1B, En
        private GpioPin m_pin_motor3A = null;
        private const int MOTOR3A_PIN = 23; //GPIO 23
        private GpioPin m_pin_motor4A = null;
        private const int MOTOR4A_PIN = 24; //GPIO 24
        private GpioPin m_pin_motor34EN = null;
        private const int MOTOR34EN_PIN = 18; //GPIO 18

        //Cadence Sensor
        //Digital Inputs
        //YellowButton
        private GpioPin m_pin_yellow_button = null;
        private const int YELLOW_BUTTON_PIN = 26; //GPIO 26
        //GreenButton
        private GpioPin m_pin_green_button = null;
        private const int GREEN_BUTTON_PIN = 13;  //GPIO 13
        //Heartrate Sensor
        private GpioPin m_pin_heart_rate_sensor = null;
        private const int HEART_RATE_SENSOR_PIN = 16; //GPIO 16
        //Cadence Sensor
        private GpioPin m_pin_cadence_sensor = null;
        private const int CADENCE_SENSOR_PIN = 27;  //GPIO 27


        //Analog Inputs
        //ADC - This uses SPI so do it last

        private const int BUTTON_DEBOUNCE = 50;

        private void InitGPIO()
        {
            m_gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (m_gpio == null)
            {
                throw new Exception("There is no GPIO controller on this device.");
            }

            m_pin_blue_led = InitLED(BLUE_LED_PIN, GpioPinValue.High);

            m_pin_red_led = InitLED(RED_LED_PIN, GpioPinValue.High);

            m_pin_yellow_button = InitButton(YELLOW_BUTTON_PIN);
            m_pin_yellow_button.ValueChanged += M_pin_yellow_button_ValueChanged;

            m_pin_green_button = InitButton(GREEN_BUTTON_PIN);
            m_pin_green_button.ValueChanged += M_pin_green_button_ValueChanged;


            InitHeartRateSensor();

            InitMotor();

            InitADC();


        }
        private GpioPin InitLED(int pin_number, GpioPinValue pin_value)
        {
            GpioPin tmp = m_gpio.OpenPin(pin_number);

            // Show an error if the pin wasn't initialized properly
            if (tmp == null)
            {
                throw new Exception(
                    string.Format("There were problems initializing the GPIO pin {0}.", pin_number));
            }

            tmp.Write(pin_value);
            tmp.SetDriveMode(GpioPinDriveMode.Output);

            return tmp;
        }


        private void InitMotor()
        {
            m_pin_motor3A = m_gpio.OpenPin(MOTOR3A_PIN);
            m_pin_motor4A = m_gpio.OpenPin(MOTOR4A_PIN);
            m_pin_motor34EN = m_gpio.OpenPin(MOTOR34EN_PIN);

            if (m_pin_motor3A == null ||
                m_pin_motor4A == null ||
                m_pin_motor34EN == null)
            {
                throw new Exception("There were problems initializing the GPIO pins for the motor.");
            }


            m_pin_motor3A.Write(GpioPinValue.Low);
            m_pin_motor4A.Write(GpioPinValue.Low);
            m_pin_motor34EN.Write(GpioPinValue.Low);
            m_pin_motor3A.SetDriveMode(GpioPinDriveMode.Output);
            m_pin_motor4A.SetDriveMode(GpioPinDriveMode.Output);
            m_pin_motor34EN.SetDriveMode(GpioPinDriveMode.Output);

        }


        private GpioPin InitButton(int pin_number)
        {
            GpioPin tmp = m_gpio.OpenPin(pin_number);


            if (tmp.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                //change this if you get rid of the external pull up
                tmp.SetDriveMode(GpioPinDriveMode.Input);
            }
            else
            {
                tmp.SetDriveMode(GpioPinDriveMode.Input);
            }

            tmp.DebounceTimeout = TimeSpan.FromMilliseconds(BUTTON_DEBOUNCE);
            

            return tmp;
        }


        


        private void InitHeartRateSensor()
        {
            m_pin_heart_rate_sensor = m_gpio.OpenPin(HEART_RATE_SENSOR_PIN);


            if (m_pin_heart_rate_sensor.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                //change this if you get rid of the external pull up
                m_pin_heart_rate_sensor.SetDriveMode(GpioPinDriveMode.Input);
            }
            else
            {
                m_pin_heart_rate_sensor.SetDriveMode(GpioPinDriveMode.Input);
            }

            m_pin_heart_rate_sensor.DebounceTimeout = TimeSpan.FromMilliseconds(BUTTON_DEBOUNCE);
            m_pin_heart_rate_sensor.ValueChanged += M_pin_heart_rate_sensor_ValueChanged;

        }

        

        private void InitCadenceSensor()
        {
            m_pin_cadence_sensor = m_gpio.OpenPin(CADENCE_SENSOR_PIN);


            if (m_pin_cadence_sensor.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                //change this if you get rid of the external pull up
                m_pin_cadence_sensor.SetDriveMode(GpioPinDriveMode.Input);
            }
            else
            {
                m_pin_cadence_sensor.SetDriveMode(GpioPinDriveMode.Input);
            }

            m_pin_cadence_sensor.DebounceTimeout = TimeSpan.FromMilliseconds(BUTTON_DEBOUNCE);
            m_pin_cadence_sensor.ValueChanged += M_pin_cadence_sensor_ValueChanged;

        }

        private async void InitADC()
        {
            //CLK - SPI0 CLK
            //Data - SPI0 MISO
            //CS - SPI0 CS

            try
            {
                var settings = new SpiConnectionSettings(0);
                settings.ClockFrequency = 1600000;                              /* 1.6MHz is the rated speed                   */
                settings.Mode = SpiMode.Mode3;                                  /* The accelerometer expects an idle-high clock polarity, we use Mode3    
                                                                         * to set the clock polarity and phase to: CPOL = 1, CPHA = 1         
                                                                         */

                string aqs = SpiDevice.GetDeviceSelector();                     /* Get a selector string that will return all SPI controllers on the system */
                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the SPI bus controller devices with our selector string             */
                SpiDevice SPIAccel = await SpiDevice.FromIdAsync(dis[0].Id, settings);    /* Create an SpiDevice with our bus controller and SPI settings             */
                if (SPIAccel == null)
                {
                    throw new Exception(string.Format(
                        "SPI Controller {0} is currently in use by " +
                        "another application. Please ensure that no other applications are using SPI.",
                        dis[0].Id));
                }
            }
            catch(Exception e) { }
        }
    }
}
