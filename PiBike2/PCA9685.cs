using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace PiBike2
{
    class PCA9685
    {

        private byte PCA9685_MODE1 = 0x00; // location for Mode1 register address
        private byte PCA9685_MODE2 = 0x01; // location for Mode2 reigster address
        private byte PCA9685_LED0 = 0x06; // location for start of LED0 registers


        private int CHANNEL_START = 0;
        private int CHANNEL_END = 15;
        private int MAX_PWM = 4095;


        private I2cDevice m_device;

        public PCA9685(I2cDevice device) {

            m_device = device;
            try
            {

                device.Write(new byte[] { PCA9685_MODE1, 0x21 });

                AllOff();

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

            }


        }

        public void AllOff()
        {
            for(int i = CHANNEL_START; i <= CHANNEL_END; i++)
            {
                SetPWM(i, 0);
            }
        }

        public void AllOn()
        {
            for (int i = CHANNEL_START; i <= CHANNEL_END; i++)
            {
                SetPWM(i, 1.0);
            }
        }

        public void All(double duty_cycle)
        {
            for (int i = CHANNEL_START; i <= CHANNEL_END; i++)
            {
                SetPWM(i, duty_cycle);
            }
        }


        public void SetPWM(int channel, double duty_cycle)
        {
            if ((channel < CHANNEL_START) || (channel > CHANNEL_END))
            {
                throw new Exception("Channel out of range");
            }

            if ((duty_cycle < 0) || (duty_cycle > 1))
            {
                throw new Exception("Percentage out of range");
            }

            byte base_addr = (byte)((PCA9685_LED0 + (4 * channel)));
            uint value = (uint)(MAX_PWM * duty_cycle);

            byte low_byte = (byte)(value);
            byte high_byte = (byte)(value >> 8);

            m_device.Write(new byte[] { base_addr, 0x00 });
            m_device.Write(new byte[] { (byte)(base_addr + 1), 0x00 });
            m_device.Write(new byte[] { (byte)(base_addr + 2), low_byte });
            m_device.Write(new byte[] { (byte)(base_addr + 3), high_byte });
        }
    }
}
