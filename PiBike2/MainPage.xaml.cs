using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PiBike2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        C7ZL m_bike;

        public MainPage()
        {
            this.InitializeComponent();

            m_bike = new C7ZL();

            m_bike.HeartRateChanged += M_bike_HeartRateChanged;

        }

        private void M_bike_HeartRateChanged(object sender, EventArgs e)
        {
            tbHeartRate.Text = m_bike.HeartRate.ToString();
        }
    }
}
