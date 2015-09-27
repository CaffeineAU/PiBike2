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

        //Digital Outputs
        //BLUE LED - this wont work because it is the SPI1 CS0 port
        private GpioPin m_pin_blue_led;
        private const int BLUE_LED_PIN = 5;
        //RED LED
        private GpioPin m_pin_red_led;
        private const int RED_LED_PIN = 22;
        //Motor 1A, 1B, En
        private GpioPin m_pin_motor3A;
        private const int MOTOR3A_PIN = 23;
        private GpioPin m_pin_motor4A;
        private const int MOTOR4A_PIN = 24;
        private GpioPin m_pin_motor34EN;
        private const int MOTOR34EN_PIN = 18;

        //Cadence Sensor
        //Digital Inputs
        //YellowButton - This wont work because it is the SPI0 SCLK pin
        private GpioPin m_pin_yellow_button;
        private const int YELLOW_BUTTON_PIN = 6;
        //GreenButton - This wont work because it is the SPI0 MISO pin
        private GpioPin m_pin_green_button;
        private const int GREEN_BUTTON_PIN = 13;
        //Heartrate Sensor - This wont work because it is the SPI1 SCLK pin
        private GpioPin m_pin_heart_rate_sensor;
        private const int HEART_RATE_SENSOR_PIN = 12;
        //Cadence Sensor
        private GpioPin m_pin_cadence_sensor;
        private const int CADENCE_SENSOR_PIN = 27;


        //Analog Inputs
        //ADC - This uses i2c so do it last

        private const int BUTTON_DEBOUNCE = 50;

        private void InitGPIO()
        {
            m_gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (m_gpio == null)
            {
                m_pin_red_led = null;
                m_pin_yellow_button = null;
                throw new Exception("There is no GPIO controller on this device.");
            }

            InitBlueLED();
            InitRedLED();
            InitYellowButton();
            InitGreenButton();
            InitHeartRateSensor();
            InitMotor();

            InitADC();


        }
        private void InitBlueLED()
        {
            m_pin_blue_led = m_gpio.OpenPin(BLUE_LED_PIN);

            // Show an error if the pin wasn't initialized properly
            if (m_pin_blue_led == null)
            {
                throw new Exception("There were problems initializing the GPIO pin for the blue LED.");
            }

            m_pin_blue_led.Write(GpioPinValue.High);
            m_pin_blue_led.SetDriveMode(GpioPinDriveMode.Output);
        }

        private void InitRedLED()
        {
            m_pin_red_led = m_gpio.OpenPin(RED_LED_PIN);

            // Show an error if the pin wasn't initialized properly
            if (m_pin_red_led == null)
            {
                throw new Exception("There were problems initializing the GPIO pin for the red LED.");
            }

            m_pin_red_led.Write(GpioPinValue.High);
            m_pin_red_led.SetDriveMode(GpioPinDriveMode.Output);
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


        private void InitYellowButton()
        {
            m_pin_yellow_button = m_gpio.OpenPin(YELLOW_BUTTON_PIN);


            if (m_pin_yellow_button.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                //change this if you get rid of the external pull up
                m_pin_yellow_button.SetDriveMode(GpioPinDriveMode.Input);
            }
            else
            {
                m_pin_yellow_button.SetDriveMode(GpioPinDriveMode.Input);
            }

            m_pin_yellow_button.DebounceTimeout = TimeSpan.FromMilliseconds(BUTTON_DEBOUNCE);
            m_pin_yellow_button.ValueChanged += M_pin_yellow_button_ValueChanged;

        }

        

        private void InitGreenButton()
        {
            m_pin_green_button = m_gpio.OpenPin(GREEN_BUTTON_PIN);


            if (m_pin_green_button.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                //change this if you get rid of the external pull up
                m_pin_green_button.SetDriveMode(GpioPinDriveMode.Input);
            }
            else
            {
                m_pin_green_button.SetDriveMode(GpioPinDriveMode.Input);
            }

            m_pin_green_button.DebounceTimeout = TimeSpan.FromMilliseconds(BUTTON_DEBOUNCE);
            m_pin_green_button.ValueChanged += M_pin_green_button_ValueChanged;

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
