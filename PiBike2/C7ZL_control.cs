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

        Timer spi_timer;

        Timer dial_poll_timer;


        

        private const int ADC_MAX = 8192;
        private const int ADC_LIMIT_GUARDBAND = 100;


        private const decimal DEAD_HALFBAND = 200; //in adc

        private const int MAX_LEVEL = 10;

        public string control_state = string.Empty;

        private int toInt(GpioPinValue val)
        {
            if(val == GpioPinValue.High)
            {
                return 1;
            }
            return 0;
        }


        

        private void MotorControlCallback(object state)
        {
            //read from the SPI device
            spi_adc.Read(ReadBuf);

            //first 3 bits of the first byte are discarded

            int raw_adc = ((ReadBuf[0] & 0x1F) << 8) + ReadBuf[1];

            int target_adc = (Difficulty * ADC_MAX) / MAX_LEVEL;

            //we don't want to go right up to the very end to protect the gears
            target_adc = Math.Min(target_adc, ADC_MAX - ADC_LIMIT_GUARDBAND);
            target_adc = Math.Max(target_adc, 0 + ADC_LIMIT_GUARDBAND);

            double percent = 1;


            control_state = string.Format("target = {0}\nactual = {1}", target_adc, raw_adc);

            //do the motor control
            if (raw_adc < (target_adc - DEAD_HALFBAND))
            {
                SetMotor(MotorState.Tighten, percent);
            }
            else if (raw_adc > (target_adc + DEAD_HALFBAND))
            {
                SetMotor(MotorState.Loosen, percent);
            }
            else
            {
                SetMotor(MotorState.Off,percent);
            }

        }

        public enum MotorState { Loosen, Tighten, Off};

        public MotorState motor_state = MotorState.Off;

        private void SetMotor(MotorState state, double percent)
        {

            if(percent < 0 || percent > 1)
            {
                throw new Exception("Percent out of range");
            }

            if (pwm_controller != null)
            {

                motor_state = state;



                //the motor controller is a simple on off right now.
                //future work will be to make the enable a PWM signal with a p-controller
                //add an extra method parameter 'speed'

                switch (state)
                {
                    case MotorState.Loosen:
                        m_motor3A.Write(GpioPinValue.High);
                        m_motor4A.Write(GpioPinValue.Low);
                        pwm_controller.SetPWM(7, percent);

                        break;
                    case MotorState.Tighten:
                        m_motor3A.Write(GpioPinValue.Low);
                        m_motor4A.Write(GpioPinValue.High);
                        pwm_controller.SetPWM(7, percent);

                        break;
                    default:
                        m_motor3A.Write(GpioPinValue.Low);
                        m_motor4A.Write(GpioPinValue.Low);
                        pwm_controller.SetPWM(7, 0);

                        break;
                }
            }

        }

    }
}
