using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PiBike2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        C7ZL m_bike;

        DispatcherTimer ui_update_timer;

        public MainPage()
        {
            this.InitializeComponent();

            m_bike = new C7ZL();

            m_bike.iFitLED = C7ZL.OnOff.On;

            m_bike.HeartRateChanged += M_bike_HeartRateChanged;
            m_bike.RpmChanged += M_bike_RpmChanged;

            ui_update_timer = new DispatcherTimer();
            ui_update_timer.Interval = new TimeSpan(0,0,0,0,100);
            ui_update_timer.Tick += Timer_Tick;
            ui_update_timer.Start();


            m_bike.Difficulty = 1;

            m_bike.DialClickedClockwise += M_bike_DialClickedClockwise;
            m_bike.DialClickedAnticlockwise += M_bike_DialClickedAnticlockwise;

            m_bike.ButtonUpPressed += M_bike_ButtonUpPressed;
            m_bike.ButtonDownPressed += M_bike_ButtonDownPressed;

        }

        private void M_bike_ButtonDownPressed(object sender, EventArgs e)
        {
            m_bike.Difficulty--;
        }

        private void M_bike_ButtonUpPressed(object sender, EventArgs e)
        {
           m_bike.Difficulty++;
        }

        private void M_bike_DialClickedAnticlockwise(object sender, EventArgs e)
        {
            m_bike.Difficulty--;
        }

        private void M_bike_DialClickedClockwise(object sender, EventArgs e)
        {
            m_bike.Difficulty++;
        }

        private void M_bike_RpmChanged(object sender, EventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbRPM_value.Text = m_bike.RPM.ToString();
            });
        }

        private void Timer_Tick(object sender, object e)
        {
            tbDifficulty_value.Text = m_bike.Difficulty.ToString();

            tbADC_value.Text = m_bike.control_state.ToString();
            //tbHR_value.Text = m_bike.motor_state.ToString();
        }

        private void M_bike_YellowButtonPressed(object sender, EventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                SetDifficulty(m_bike.Difficulty + 1);
            });
        }

        private void M_bike_GreenButtonPressed(object sender, EventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                SetDifficulty(m_bike.Difficulty - 1);
            });
        }

        private void M_bike_HeartRateChanged(object sender, EventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                tbHR_value.Text = m_bike.HeartRate.ToString();
            });
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            SetDifficulty(m_bike.Difficulty + 1);
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            SetDifficulty(m_bike.Difficulty - 1);
        }


        private void SetDifficulty(int value)
        {
            try
            {
                m_bike.Difficulty = value;
            }
            catch(Exception e)
            {
                lblError.Text = string.Format("{0} {1}", DateTime.Now, e.Message);
            }

        }

        private void btnOff_Click(object sender, RoutedEventArgs e)
        {

            m_bike.iFitLED = C7ZL.OnOff.Off;

            ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, new TimeSpan(0));
        }

    }
}
