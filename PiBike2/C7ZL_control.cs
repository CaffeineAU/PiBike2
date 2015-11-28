using System;
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

        Timer spi_timer;

        private const int ADC_MAX = 8192;
        private const decimal DEAD_HALFBAND = 200; //in adc

        public string control_state = string.Empty;

        private void TimerCallback(object state)
        {
            //read from the SPI device
            spi_adc.Read(ReadBuf);

            //first 3 bits of the first byte are discarded

            int raw_adc = ((ReadBuf[0] & 0x1F) << 8) + ReadBuf[1];

            int target_adc = Difficulty * ADC_MAX / 10;


            control_state = string.Format("target = {0}\nactual = {1}", target_adc, raw_adc);

            //do the motor control
            if (raw_adc < (target_adc - DEAD_HALFBAND))
            {
                SetMotor(MotorState.Tighten);
            }
            else if (raw_adc > (target_adc + DEAD_HALFBAND))
            {
                SetMotor(MotorState.Loosen);
            }
            else
            {
                SetMotor(MotorState.Off);
            }

        }

        public enum MotorState { Loosen, Tighten, Off};

        public MotorState motor_state = MotorState.Off;

        private void SetMotor(MotorState state)
        {

            motor_state = state;

            //the motor controller is a simple on off right now.
            //future work will be to make the enable a PWM signal with a p-controller
            //add an extra method parameter 'speed'

            switch (state)
            {
                case MotorState.Loosen:
                        m_pin_motor3A.Write(GpioPinValue.High);
                        m_pin_motor4A.Write(GpioPinValue.Low);
                        m_pin_motor34EN.Write(GpioPinValue.High);

                        m_pin_red_led.Write(GpioPinValue.Low);
                    break;
                case MotorState.Tighten:
                        m_pin_motor3A.Write(GpioPinValue.Low);
                        m_pin_motor4A.Write(GpioPinValue.High);
                        m_pin_motor34EN.Write(GpioPinValue.High);

                        m_pin_red_led.Write(GpioPinValue.High);
                        m_pin_blue_led.Write(GpioPinValue.Low);
                    break;
                default:
                    m_pin_motor3A.Write(GpioPinValue.Low);
                    m_pin_motor4A.Write(GpioPinValue.Low);
                    m_pin_motor34EN.Write(GpioPinValue.Low);

                    m_pin_red_led.Write(GpioPinValue.High);
                    m_pin_blue_led.Write(GpioPinValue.High);
                    break;
            }

        }

    }
}
