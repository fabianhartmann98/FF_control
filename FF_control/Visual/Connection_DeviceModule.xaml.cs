using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using FF_control.Bluetooth;
using System.Windows.Media.Animation;


namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Connection_DeviceModule.xaml
    /// </summary>
    public partial class Connection_DeviceModule : UserControl
    {
        public Connection_DeviceModule(BluetoothDeviceInfo device)
        {
            InitializeComponent();
            DeviceName = device.DeviceName;
            DeviceAddress = device.DeviceAddress.ToString();
            ClassOfDevice = device.ClassOfDevice.Device.ToString();
            Connected = "not connected";
            if (device.Connected)
                Connected = "Connected";
            wp.DataContext = this;
        }

        public string DeviceName { get; set; }
        public string DeviceAddress { get; set; }
        public string ClassOfDevice { get; set; }
        public string Connected { get; set; }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //((Storyboard)FindResource("sb")).Begin();
        }
    }
}
