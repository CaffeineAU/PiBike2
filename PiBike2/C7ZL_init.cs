using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Devices.Spi;

namespace PiBike2
{
    partial class C7ZL
    {
        GpioController m_gpio;

        //GPIO Numbers
        //Warning.  Microsoft reserved GPIO 17, 19, 20, and 21 for SPI1
        //Do NOT use for GPIO
        private const int BUTTON_UP = 5;
        private const int WHEEL_3 = 6;
        private const int BUTTON_DOWN = 12;
        private const int WHEEL_2 = 26;
        private const int HEART_RATE_SENSOR_PIN = 16;
        //DO NOT USE 17
        private const int MOTOR34EN_PIN = 18;
        //DO NOT USE 19
        //DO NOT USE 20
        //DO NOT USE 21
        private const int MOTOR3A_PIN = 23;
        private const int MOTOR4A_PIN = 24;
        private const int IFIT_LED_PIN = 25; 
        private const int WHEEL_1 = 13; 
        private const int CADENCE_SENSOR_PIN = 27;  




        //Digital Outputs
        //RED LED
        private GpioPin m_ifit_led = null;
        private GpioPin m_motor3A = null;
        private GpioPin m_motor4A = null;
        private GpioPin m_motor34EN = null;
        
        //Digital Inputs
        private GpioPin m_wheel_1 = null;
        private GpioPin m_wheel_2 = null;
        private GpioPin m_wheel_3 = null;
        private GpioPin m_button_up = null;
        private GpioPin m_button_down = null;
        private GpioPin m_heart_rate_sensor = null;
        private GpioPin m_cadence_sensor = null;
        


        //Analog Inputs
        //ADC - This uses SPI so do it last
        SpiDevice spi_adc = null;
        byte[] ReadBuf = new byte[2];

        private const int BUTTON_DEBOUNCE = 100;

        
       /* private void TestInputs()
        {

            int[] channels = new int []{ 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 ,27};

            for(int i = 0; i < channels.Length; i++)
            {

                int io = channels[i];

                try {

                    
                    GpioPin tmp = m_gpio.OpenPin(io);


                    Debug.WriteLine("{0} Up [{1}], Down[{2}]", io, tmp.IsDriveModeSupported(GpioPinDriveMode.InputPullUp), tmp.IsDriveModeSupported(GpioPinDriveMode.InputPullDown));

                    if (tmp.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                    {
                        tmp.SetDriveMode(GpioPinDriveMode.InputPullUp);


                        Debug.WriteLine(string.Format("{0} InputPullup {1}", io, tmp.Read()));
                    }


                    if (tmp.IsDriveModeSupported(GpioPinDriveMode.InputPullDown))
                    {
                        tmp.SetDriveMode(GpioPinDriveMode.InputPullDown);

                        Debug.WriteLine(string.Format("{0} InputPullDown {1}", io, tmp.Read()));
                    }

                    tmp.Dispose();

                }
                catch(Exception e)
                {
                    Debug.WriteLine("{0} {1}", io, e.Message);
                }

            }

        }*/

        private void InitGPIO()
        {
            m_gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (m_gpio == null)
            {
                throw new Exception("There is no GPIO controller on this device.");
            }


            m_ifit_led = InitLED(IFIT_LED_PIN, GpioPinValue.High);

            //wheel 1 and 2 are the encoder inputs so do not link to an event, use polling to get the speed needed
            m_wheel_1 = InitButton(WHEEL_1);
            m_wheel_2 = InitButton(WHEEL_2);
            dial_poll_timer = new Timer(this.DialPollCallback, null, 0, 1);


            m_wheel_3 = InitButton(WHEEL_3);
            m_wheel_3.ValueChanged += M_wheel_3_ValueChanged;


            m_button_down = InitButton(BUTTON_DOWN);
            m_button_down.ValueChanged += M_button_down_ValueChanged;

            m_button_up = InitButton(BUTTON_UP);
            m_button_up.ValueChanged += M_button_up_ValueChanged;

            InitCadenceSensor();
            InitHeartRateSensor();

            InitMotor();

            InitADC();

            InitI2C(0x70);
            

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
            m_motor3A = m_gpio.OpenPin(MOTOR3A_PIN);
            m_motor4A = m_gpio.OpenPin(MOTOR4A_PIN);
            m_motor34EN = m_gpio.OpenPin(MOTOR34EN_PIN);

            if (m_motor3A == null ||
                m_motor4A == null ||
                m_motor34EN == null)
            {
                throw new Exception("There were problems initializing the GPIO pins for the motor.");
            }


            m_motor3A.Write(GpioPinValue.Low);
            m_motor4A.Write(GpioPinValue.Low);
            m_motor34EN.Write(GpioPinValue.Low);
            m_motor3A.SetDriveMode(GpioPinDriveMode.Output);
            m_motor4A.SetDriveMode(GpioPinDriveMode.Output);
            m_motor34EN.SetDriveMode(GpioPinDriveMode.Output);

        }


        private GpioPin InitButton(int pin_number)
        {
            GpioPin tmp = m_gpio.OpenPin(pin_number);


            if (tmp.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                //change this if you get rid of the external pull up
                tmp.SetDriveMode(GpioPinDriveMode.InputPullUp);
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
            m_heart_rate_sensor = m_gpio.OpenPin(HEART_RATE_SENSOR_PIN);


            if (m_heart_rate_sensor.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                //change this if you get rid of the external pull up
                m_heart_rate_sensor.SetDriveMode(GpioPinDriveMode.Input);
            }
            else
            {
                m_heart_rate_sensor.SetDriveMode(GpioPinDriveMode.Input);
            }

            m_heart_rate_sensor.DebounceTimeout = TimeSpan.FromMilliseconds(0);
            m_heart_rate_sensor.ValueChanged += M_heart_rate_sensor_ValueChanged;

        }

        

        private void InitCadenceSensor()
        {
            m_cadence_sensor = m_gpio.OpenPin(CADENCE_SENSOR_PIN);


            if (m_cadence_sensor.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            { 
                
                m_cadence_sensor.SetDriveMode(GpioPinDriveMode.InputPullUp);
            }
            else
            {
                m_cadence_sensor.SetDriveMode(GpioPinDriveMode.Input);
            }

            m_cadence_sensor.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            m_cadence_sensor.ValueChanged += M_cadence_sensor_ValueChanged;

            RPM = 0;


        }

        private async void InitADC()
        {
            //CLK - SPI0 CLK
            //Data - SPI0 MISO
            //CS - SPI0 CS

            var settings = new SpiConnectionSettings(0);
            settings.ClockFrequency = 1600000;                              /* 1.6MHz is the rated speed                   */
            settings.Mode = SpiMode.Mode0;                                  /* The accelerometer expects an idle-high clock polarity, we use Mode3    
                                                                         * to set the clock polarity and phase to: CPOL = 1, CPHA = 1         
                                                                         */

            string aqs = SpiDevice.GetDeviceSelector();                     /* Get a selector string that will return all SPI controllers on the system */
            var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the SPI bus controller devices with our selector string             */
            spi_adc = await SpiDevice.FromIdAsync(dis[0].Id, settings);    /* Create an SpiDevice with our bus controller and SPI settings             */
            if (spi_adc == null)
            {
                throw new Exception(string.Format(
                    "SPI Controller {0} is currently in use by " +
                    "another application. Please ensure that no other applications are using SPI.",
                    dis[0].Id));
            }
            else
            {
                spi_timer = new Timer(this.MotorControlCallback, null, 0, 100);
            }

        }

        private I2cDevice i2cPwmController;

        private byte PCA9685_MODE1 = 0x00; // location for Mode1 register address
        private byte PCA9685_MODE2 = 0x01; // location for Mode2 reigster address
        private byte PCA9685_LED0 = 0x06; // location for start of LED0 registers

        private byte PCA9685_I2C_BASE_ADDRESS = 0x40; //b1000 000 - note this is 7 bit

        //private byte _i2cAddress;

        private async void InitI2C(byte address)
        {
            try
            {

                //byte[] buf = new byte[3];
                //buf[0] = (byte)(address << 1);
                //buf[1] = PCA9685_MODE1;
                //buf[2] = 0x21;


                byte _i2cAddress = 0x40; //all zero.  fixed bit is bit 7

                var i2cSettings = new I2cConnectionSettings(_i2cAddress);
                i2cSettings.BusSpeed = I2cBusSpeed.FastMode;

                string deviceSelector = I2cDevice.GetDeviceSelector("I2C1");

                var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);

                i2cPwmController = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);


                i2cPwmController.Write(new byte[] { PCA9685_MODE1, 0x21 });


                //reset
                //i2cPwmController.Write(new byte[] { (byte)0x01 });
                //await Task.Delay(10);

                //check it can be read from

                /*byte mode1_reg = readRegister(PCA9685_MODE1);
                Debug.WriteLine("Mode 1 = {0}", ToBinary(mode1_reg));
                byte mode2_reg = readRegister(PCA9685_MODE2);
                Debug.WriteLine("Mode 2 = {0}", ToBinary(mode2_reg));


                //reset
                Debug.WriteLine("Perform Reset");
                i2cPwmController.Write(new byte[] { PCA9685_MODE1, (byte)(mode1_reg | 0x80) });
                mode1_reg = readRegister(PCA9685_MODE1);
                Debug.WriteLine("Mode 1 = {0}", ToBinary(mode1_reg));



                //set to output

                Debug.WriteLine("Turn on output");
                i2cPwmController.Write(new byte[] { PCA9685_MODE2, (byte)(mode2_reg | (byte)0x01) });
                mode2_reg = readRegister(PCA9685_MODE2);
                Debug.WriteLine("Mode 2 = {0}", ToBinary(mode2_reg));
                */

                await Task.Delay(10);


              

                

                await Task.Delay(10);


                //13 = red
                

                //writeLED(14, 0x00, 0x00, 0x10, 0x00);
                //await Task.Delay(10);
                //writeLED(15, 0x10, 0x00, 0x10, 0x00);
                for (int i = 13; i <= 15; i++)
                {
                    Debug.WriteLine("LED {0}", i);
                    for (int j = 0; j < 4; j++)
                    {
                        byte addr = (byte)(PCA9685_LED0 + (4 * i) + j);
                        
                        Debug.WriteLine("{0} = {1}", addr, readRegister(addr));
                    }
                    

                }
                //int y = 0;  

                List<Tuple<bool, bool, bool>> list = new List<Tuple<bool, bool, bool>>();
                list.Add(new Tuple<bool, bool, bool>(false, false, false));
                list.Add(new Tuple<bool, bool, bool>(false, false, true));
                list.Add(new Tuple<bool, bool, bool>(false, true, false));
                list.Add(new Tuple<bool, bool, bool>(false, true, true));
                list.Add(new Tuple<bool, bool, bool>(true, false, false));
                list.Add(new Tuple<bool, bool, bool>(true, false, true));
                list.Add(new Tuple<bool, bool, bool>(true, true, false));
                list.Add(new Tuple<bool, bool, bool>(true, true, true));

                foreach(Tuple<bool,bool,bool> t in list)
                {

                    writeLED(13, t.Item1); //red, false = on, true = green on
                    writeLED(14, t.Item2); // true -= blue on
                    writeLED(15, t.Item3); //green or blue?

                    Debug.WriteLine("Combination = {0}, {1}, {2}", t.Item1, t.Item2, t.Item3);
                    

                    await Task.Delay(2000);
                }

            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);

                int i = 0;
            }



        }

        private string ToBinary(byte b)
        {
            return Convert.ToString(b, 2);
        }


        private byte readRegister(byte address)
        {

            byte[] ret = new byte[1];


            i2cPwmController.WriteRead(new byte[] { address }, ret);

            return ret[0];

        }


        private void writeLED(int led_num, bool state)
        {

            //I think the ratio of on byte to low byte give the PWM ratio
            //so for on it would be 0b1000 and 0b0000 to give all on and none off


            if(led_num >= 0 && led_num <= 15)
            {

                byte base_addr = (byte)((PCA9685_LED0 + (4 * led_num)));

                if (state)
                {
                    i2cPwmController.Write(new byte[] { base_addr, 0x00 });
                    i2cPwmController.Write(new byte[] { (byte)(base_addr + 1), 0x00 });
                    i2cPwmController.Write(new byte[] { (byte)(base_addr + 2), 0xFF });
                    i2cPwmController.Write(new byte[] { (byte)(base_addr + 3), 0x0F });

                }
                else
                {
                    i2cPwmController.Write(new byte[] { base_addr, 0x00 });
                    i2cPwmController.Write(new byte[] { (byte)(base_addr + 1), 0x00 });
                    i2cPwmController.Write(new byte[] { (byte)(base_addr + 2), 0x00 });
                    i2cPwmController.Write(new byte[] { (byte)(base_addr + 3), 0x00 });

                }
            }
        }

        

    }
}
