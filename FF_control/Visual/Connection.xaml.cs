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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using FF_control.Bluetooth;

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class Connection : UserControl
    {
        public BT_connection bt;

        public Connection()
        {
            InitializeComponent();
            bt = new BT_connection();
            bt.DeviceConnected += bt_DeviceConnected;
            bt.DiscoverDevicesEnded += bt_DiscoverDevicesEnded;
            bt.DeviceDisconnected += bt_DeviceDisconnected;

            bt.GetAvailableDevicesAsync();
        }

        

        void bt_DiscoverDevicesEnded(object sender, EventArgs e)
        {
            if (!stackpanel.Dispatcher.CheckAccess())
            {
                stackpanel.Dispatcher.Invoke(
                    (Action<object, EventArgs>)bt_DiscoverDevicesEnded, sender, e);
            }
            else
            {
                stackpanel.Children.Clear();

                foreach (var item in bt.infos)
                {
                    Connection_DeviceModule cdm = new Connection_DeviceModule(item);
                    cdm.Dis_ConnectDevice += cdm_Dis_ConnectDevice;
                    stackpanel.Children.Add(cdm);
                }
            }
        }

        void cdm_Dis_ConnectDevice(object sender, RoutedEventArgs e)
        {
            Connection_DeviceModule cdm = (Connection_DeviceModule)((Button)sender).Tag;
            if (cdm.Device.Connected)
                bt.DisconnectFromDevice();
            else
                bt.ConnectToDevice(cdm.Device);           
        }

        void bt_DeviceConnected(object sender, EventArgs e)
        {
            stackpanel.Children.Clear();
            stackpanel.Children.Add(new Connection_DeviceModule(bt.ConnectedDevice));
        }

        void bt_DeviceDisconnected(object sender, EventArgs e)
        {
            bt.GetAvailableDevicesAsync();
        }

        private void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            bt.GetAvailableDevicesAsync();
        }
    }
}
